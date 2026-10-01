using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;
public class SessionLoginInput {
    [Required] public string UserName { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public DeclarationAcceptance? Declaration { get; set; }
}
public class SessionRegisterInput : SessionLoginInput {
    [Required,EmailAddress] public string Email { get; set; } = "";
    [Required,MaxLength(100)] public string Name { get; set; } = "";
    [Required,RegularExpression("^(Patient|Practitioner)$")] public string Role { get; set; } = "Patient";
    [Required,RegularExpression("^[0-9]{9}$")] public string TajNumber { get; set; } = "";
    public DateOnly BirthDate { get; set; }
    [MaxLength(100)] public string Specialty { get; set; } = "Orvos";
}
[ApiController,Route("api/session")]
public class SessionController(UserManager<AppUser> users,SignInManager<AppUser> signIn,AppDbContext db):ControllerBase {
    [HttpGet("declaration"), AllowAnonymous]
    public IActionResult Declaration([FromQuery] string role = "Patient") =>
        role is "Patient" or "Practitioner" or "AdmissionsOffice"
            ? Ok(RegistrationDeclarations.ForRoles([role]))
            : BadRequest(new { message = "Ismeretlen nyilatkozati szerepkör." });

    [HttpPost("login"),AllowAnonymous]
    public async Task<IActionResult> Login(SessionLoginInput input) {
        var user=await users.FindByNameAsync(input.UserName) ?? await users.FindByEmailAsync(input.UserName);
        if(user is null)return Unauthorized(new{message="Hibás felhasználónév vagy jelszó."});
        var result=await signIn.CheckPasswordSignInAsync(user,input.Password,true);
        if(!result.Succeeded)return Unauthorized(new{message="Hibás belépési adatok vagy zárolt fiók."});
        if (user.Id.StartsWith(DemoSessionSetup.Prefix) && !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()) return Unauthorized();
        var roles = await users.GetRolesAsync(user);
        var policy = RegistrationDeclarations.ForRoles(roles);
        if (policy is not null && !await RegistrationDeclarations.HasAccepted(db, user.Id, policy))
        {
            var error = RegistrationDeclarations.Validate(policy, input.Declaration);
            if (error is not null) return Conflict(new { code = "declaration_required", message = error, declaration = policy });
            var signedName = policy.RequiresSignature ? input.Declaration!.SignedName :
                await db.Patients.Where(p => p.UserId == user.Id).Select(p => p.Name).FirstOrDefaultAsync() ?? user.UserName ?? user.Id;
            RegistrationDeclarations.Record(db, user.Id, policy, signedName, roles);
            await db.SaveChangesAsync();
        }
        return SignIn(await signIn.CreateUserPrincipalAsync(user),IdentityConstants.BearerScheme);
    }
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout() {
        var user = await users.GetUserAsync(User);
        if (user is not null) MedSetup.Check(await users.UpdateSecurityStampAsync(user));
        await signIn.SignOutAsync();
        return NoContent();
    }
    [HttpPost("register"),AllowAnonymous]
    public async Task<IActionResult> Register(SessionRegisterInput input) {
        var policy = RegistrationDeclarations.ForRoles([input.Role])!;
        var declarationError = RegistrationDeclarations.Validate(policy, input.Declaration);
        if (declarationError is not null) return BadRequest(new { message = declarationError });
        if(input.Role=="Patient" && (input.BirthDate==default || input.BirthDate>DateOnly.FromDateTime(DateTime.Today)))return BadRequest(new{message="Érvényes születési dátum szükséges."});
        if ((input.Role == "Patient" && await db.Patients.AnyAsync(p => p.TajNumber == input.TajNumber)) ||
            (input.Role == "Practitioner" && await db.Practitioners.AnyAsync(p => p.TajNumber == input.TajNumber)))
            return Conflict(new { message = "Ezzel a TAJ-számmal már létezik profil. Kérj fiók-összekapcsolást az admintól." });
        await using var tx=await db.Database.BeginTransactionAsync();
        var user=new AppUser{UserName=input.UserName.Trim(),Email=input.Email.Trim()};
        var result=await users.CreateAsync(user,input.Password);
        if(!result.Succeeded)return BadRequest(new{message=string.Join(" ",result.Errors.Select(e=>e.Code switch {
            "DuplicateUserName" => "Ez a felhasználónév már foglalt.",
            "DuplicateEmail" => "Ez az e-mail-cím már használatban van.",
            "PasswordTooShort" => "A jelszó túl rövid.",
            "PasswordRequiresNonAlphanumeric" => "A jelszónak speciális karaktert is tartalmaznia kell.",
            "PasswordRequiresDigit" => "A jelszónak számot is tartalmaznia kell.",
            "PasswordRequiresUpper" => "A jelszónak nagybetűt is tartalmaznia kell.",
            "PasswordRequiresLower" => "A jelszónak kisbetűt is tartalmaznia kell.",
            _ => e.Description
        }))});
        MedSetup.Check(await users.AddToRoleAsync(user,input.Role));
        var signedName = policy.RequiresSignature ? input.Declaration!.SignedName : input.Name;
        var acceptedAt = RegistrationDeclarations.Record(db, user.Id, policy, signedName, [input.Role]);
        string? patientId = null, practitionerId = null;
        if (input.Role == "Patient")
        {
            var profile = new Patient { Name = input.Name.Trim(), TajNumber = input.TajNumber, UserId = user.Id, BirthDate = input.BirthDate, Email = input.Email };
            db.Patients.Add(profile); patientId = profile.Id;
        }
        else
        {
            var profile = new PractitionerProfile { Name = input.Name.Trim(), TajNumber = input.TajNumber, UserId = user.Id, Specialty = input.Specialty };
            profile.BookingSettings = new() { PractitionerId = profile.Id };
            db.Practitioners.Add(profile); practitionerId = profile.Id;
        }
        var welcome = await RegistrationWelcome.Add(db, user.Id, input.Name, policy, signedName, acceptedAt, patientId, practitionerId);
        await db.SaveChangesAsync();await tx.CommitAsync();return StatusCode(201,new{user.Id, welcomeActivityId = welcome.Id});
    }
    [HttpGet("me"),Authorize]
    public async Task<IActionResult> Me() {
        var user=await users.GetUserAsync(User);if(user is null)return Unauthorized();
        return Ok(new{user.Id,user.UserName,user.Email,roles=await users.GetRolesAsync(user)});
    }
}
public static class SessionValidation {
    public static IApplicationBuilder UseMedSessionValidation(this IApplicationBuilder app)=>app.Use(async(context,next)=>{
        if(context.User.Identity?.IsAuthenticated==true) {
            context.RequestServices.GetRequiredService<AppDbContext>().AuditUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users=context.RequestServices.GetRequiredService<UserManager<AppUser>>();
            var user=await users.GetUserAsync(context.User);
            var claim=users.Options.ClaimsIdentity.SecurityStampClaimType;
            if(user is null || context.User.FindFirstValue(claim)!=await users.GetSecurityStampAsync(user) || await users.IsLockedOutAsync(user)) {
                context.Response.StatusCode=401;await context.Response.WriteAsJsonAsync(new{message="A munkamenet lejárt. Jelentkezz be újra."});return;
            }
        }
        await next();
    });
}

