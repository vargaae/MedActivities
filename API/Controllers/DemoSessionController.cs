using System.Net;
using System.Security.Claims;
using API.Med;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Persistence;
using Persistence.Identity;
namespace API.Controllers;

[ApiController,Route("api/dev-session"),AllowAnonymous]
public class DemoSessionController(IWebHostEnvironment environment,AppDbContext db,
    UserManager<AppUser> users,RoleManager<IdentityRole> roles,SignInManager<AppUser> signIn) : ControllerBase
{
    bool Enabled {
        get {
            var ip=HttpContext.Connection.RemoteIpAddress;
            return environment.IsDevelopment() && ip is not null &&
                (IPAddress.IsLoopback(ip) || (ip.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(ip.MapToIPv4())));
        }
    }
    [HttpGet]public IActionResult State()=>Ok(new {enabled=Enabled});

    [HttpPost("{role}")]
    public async Task<IActionResult> Login(string role)
    {
        if(!Enabled)return NotFound();
        if(!DemoSessionSetup.Roles.Contains(role))return BadRequest(new {message="Ismeretlen demószerepkör."});
        await DemoSessionSetup.Gate.WaitAsync(HttpContext.RequestAborted);
        try {await DemoSessionSetup.SeedAsync(db,users,roles);}
        finally {DemoSessionSetup.Gate.Release();}
        var user=await users.FindByIdAsync(DemoSessionSetup.Prefix+role);
        var principal=await signIn.CreateUserPrincipalAsync(user!);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(DemoSessionSetup.Claim,"true"));
        return SignIn(principal,IdentityConstants.BearerScheme);
    }
}
