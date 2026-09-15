using System.ComponentModel.DataAnnotations;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;
public class ManagedUserInput {
    [Required,MaxLength(100)] public string UserName { get; set; } = "";
    [Required,EmailAddress] public string Email { get; set; } = "";
    [Required,MaxLength(100)] public string Name { get; set; } = "";
    [Required,RegularExpression("^(Admin|AdmissionsOffice|Practitioner|Patient)$")] public string Role { get; set; } = "Patient";
    public string? Password { get; set; }
    public bool Disabled { get; set; }
}
[ApiController,Route("api/user-management"),Authorize(Roles="Admin")]
public class UserManagementController(UserManager<AppUser> users,AppDbContext db,AccessService access):ControllerBase {
    [HttpGet]public async Task<IActionResult> List() {
        var result=new List<object>();
        foreach(var u in await users.Users.OrderBy(u=>u.UserName).ToListAsync())result.Add(new{u.Id,u.UserName,u.Email,name=(await db.AdminProfiles.Where(p=>p.UserId==u.Id).Select(p=>p.Name).FirstOrDefaultAsync()) ?? (await db.AdmissionsOfficeProfiles.Where(p=>p.UserId==u.Id).Select(p=>p.Name).FirstOrDefaultAsync()) ?? u.UserName,roles=await users.GetRolesAsync(u),disabled=await users.IsLockedOutAsync(u)});
        return Ok(result);
    }
    [HttpPost]public async Task<IActionResult> Create(ManagedUserInput input) {
        if(string.IsNullOrWhiteSpace(input.Password))return BadRequest(new{message="Új felhasználóhoz jelszó szükséges."});
        await using var tx=await db.Database.BeginTransactionAsync();
        var u=new AppUser{UserName=input.UserName.Trim(),Email=input.Email.Trim(),LockoutEnabled=true};
        var r=await users.CreateAsync(u,input.Password);if(!r.Succeeded)return Errors(r);
        MedSetup.Check(await users.AddToRoleAsync(u,input.Role));
        await SetStaffProfile(u.Id,input);
        if(input.Disabled)MedSetup.Check(await users.SetLockoutEndDateAsync(u,DateTimeOffset.MaxValue));
        await db.SaveChangesAsync();await tx.CommitAsync();return StatusCode(201,new{u.Id});
    }
    [HttpPut("{id}")]public async Task<IActionResult> Edit(string id,ManagedUserInput input) {
        var u=await users.FindByIdAsync(id);if(u is null)return NotFound();
        if(id.StartsWith(DemoSessionSetup.Prefix))return Conflict(new{message="A demófiókok nem szerkeszthetők."});
        var roles=await users.GetRolesAsync(u);
        if(id==access.UserId && (input.Disabled || input.Role!="Admin"))return Conflict(new{message="A saját adminhozzáférésed nem vonható vissza."});
        if(roles.Contains("Admin") && (input.Role!="Admin" || input.Disabled) && (await users.GetUsersInRoleAsync("Admin")).Count(x=>x.Id!=id && (!x.LockoutEnd.HasValue || x.LockoutEnd<=DateTimeOffset.UtcNow))==0)return Conflict(new{message="Az utolsó aktív admin nem tiltható le."});
        if((input.Role!="Patient" && await db.Patients.AnyAsync(p=>p.UserId==id)) || (input.Role!="Practitioner" && await db.Practitioners.AnyAsync(p=>p.UserId==id)))return Conflict(new{message="A kapcsolt páciens/kezelőprofil miatt a szerepkör nem cserélhető."});
        await using var tx=await db.Database.BeginTransactionAsync();
        u.UserName=input.UserName.Trim();u.Email=input.Email.Trim();u.LockoutEnabled=true;
        var result=await users.UpdateAsync(u);if(!result.Succeeded)return Errors(result);
        MedSetup.Check(await users.RemoveFromRolesAsync(u,roles));MedSetup.Check(await users.AddToRoleAsync(u,input.Role));
        MedSetup.Check(await users.SetLockoutEndDateAsync(u,input.Disabled?DateTimeOffset.MaxValue:null));
        if(!string.IsNullOrEmpty(input.Password)) {
            var reset=await users.GeneratePasswordResetTokenAsync(u);
            result=await users.ResetPasswordAsync(u,reset,input.Password);if(!result.Succeeded)return Errors(result);
        }
        await SetStaffProfile(id,input);MedSetup.Check(await users.UpdateSecurityStampAsync(u));
        await db.SaveChangesAsync();await tx.CommitAsync();return NoContent();
    }
    [HttpDelete("{id}")]public async Task<IActionResult> Delete(string id) {
        if(id==access.UserId || id.StartsWith(DemoSessionSetup.Prefix))return Conflict(new{message="A saját vagy demófelhasználó nem törölhető."});
        var u=await users.FindByIdAsync(id);if(u is null)return NotFound();
        if(await db.Patients.AnyAsync(p=>p.UserId==id)||await db.Practitioners.AnyAsync(p=>p.UserId==id))return Conflict(new{message="Előbb a kapcsolt páciens/kezelőprofilt kell kezelni. A fiók letiltható."});
        if(await db.PatientNotes.AnyAsync(n=>n.AuthorUserId==id) || await db.PatientDocuments.AnyAsync(d=>d.UploadedByUserId==id))
            return Conflict(new{message="A felhasználóhoz egészségügyi feljegyzés vagy dokumentum tartozik; a fiók törlés helyett letiltható."});
        if(await users.IsInRoleAsync(u,"Admin") && (await users.GetUsersInRoleAsync("Admin")).Count(x=>x.Id!=id && (!x.LockoutEnd.HasValue || x.LockoutEnd<=DateTimeOffset.UtcNow))==0)return Conflict(new{message="Az utolsó aktív admin nem törölhető."});
        await using var tx=await db.Database.BeginTransactionAsync();
        db.AdminProfiles.RemoveRange(await db.AdminProfiles.Where(p=>p.UserId==id).ToListAsync());
        db.AdmissionsOfficeProfiles.RemoveRange(await db.AdmissionsOfficeProfiles.Where(p=>p.UserId==id).ToListAsync());
        try
        {
            await db.SaveChangesAsync();
            var result=await users.DeleteAsync(u);
            if(!result.Succeeded)
                return Conflict(new { message="A felhasználó nem törölhető: " + string.Join(" ",result.Errors.Select(HungarianError)) });
            await tx.CommitAsync();return NoContent();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException)
        { return Conflict(new { message="A felhasználó nem törölhető, mert még kapcsolódó adat hivatkozik rá. Használd a letiltást." }); }
        catch (Microsoft.Data.SqlClient.SqlException)
        { return Conflict(new { message="A felhasználó törlése nem hajtható végre. Használd a letiltást, vagy kezeld előbb a kapcsolódó adatokat." }); }
    }
    IActionResult Errors(IdentityResult r)=>BadRequest(new{message=string.Join(" ",r.Errors.Select(HungarianError))});
    static string HungarianError(IdentityError error)=>error.Code switch {
        "DuplicateUserName" => "Ez a felhasználónév már foglalt.",
        "DuplicateEmail" => "Ez az e-mail-cím már használatban van.",
        "PasswordTooShort" => "A jelszó túl rövid.",
        "PasswordRequiresNonAlphanumeric" => "A jelszónak speciális karaktert is tartalmaznia kell.",
        "PasswordRequiresDigit" => "A jelszónak számot is tartalmaznia kell.",
        "PasswordRequiresUpper" => "A jelszónak nagybetűt is tartalmaznia kell.",
        "PasswordRequiresLower" => "A jelszónak kisbetűt is tartalmaznia kell.",
        _ => error.Description
    };
    async Task SetStaffProfile(string id,ManagedUserInput input) {
        var admin=await db.AdminProfiles.SingleOrDefaultAsync(p=>p.UserId==id);
        var office=await db.AdmissionsOfficeProfiles.SingleOrDefaultAsync(p=>p.UserId==id);
        if(input.Role=="Admin") {if(admin is null)db.AdminProfiles.Add(new(){UserId=id,Name=input.Name});else admin.Name=input.Name;}
        else if(admin is not null)db.AdminProfiles.Remove(admin);
        if(input.Role=="AdmissionsOffice") {if(office is null)db.AdmissionsOfficeProfiles.Add(new(){UserId=id,Name=input.Name});else office.Name=input.Name;}
        else if(office is not null)db.AdmissionsOfficeProfiles.Remove(office);
    }
}

