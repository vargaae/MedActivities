using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;
[ApiController,Route("api/med-accounts"),Authorize]
public class AccountsController(UserManager<AppUser> users,AppDbContext db,AccessService access):ControllerBase
{
    [HttpGet("me")]public IActionResult Me()=>Ok(new{userId=access.UserId,roles=access.User.Claims.Where(c=>c.Type==System.Security.Claims.ClaimTypes.Role).Select(c=>c.Value)});
    [HttpGet,Authorize(Roles="Admin")]
    public async Task<IActionResult> List()=>Ok(await users.Users.Select(u=>new{u.Id,u.Email}).ToListAsync());
    [HttpPut("{id}/role"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Role(string id,RoleInput input) {
        if(!AppRoles.All.Contains(input.Role))return BadRequest("Ismeretlen role.");
        var u=await users.FindByIdAsync(id);if(u is null)return NotFound();
        await using var tx=await db.Database.BeginTransactionAsync();
        var result=await users.AddToRoleAsync(u,input.Role);
        if(!result.Succeeded && !await users.IsInRoleAsync(u,input.Role))return BadRequest(result.Errors.Select(e=>e.Description));
        if(input.Role=="Admin"&&!await db.AdminProfiles.AnyAsync(p=>p.UserId==id))db.AdminProfiles.Add(new(){UserId=id,Name=input.Name});
        if(input.Role=="AdmissionsOffice"&&!await db.AdmissionsOfficeProfiles.AnyAsync(p=>p.UserId==id))db.AdmissionsOfficeProfiles.Add(new(){UserId=id,Name=input.Name});
        MedSetup.Check(await users.UpdateSecurityStampAsync(u));await db.SaveChangesAsync();await tx.CommitAsync();return NoContent();
    }
}
