using System.Security.Claims;
using Application.Comments;
using API.Med;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Persistence.Identity;
namespace API.SignalR;

[Authorize]
public class ChatHub(IMediator mediator, UserManager<AppUser> users, IWebHostEnvironment environment) : Hub
{
    string ActivityId => Context.GetHttpContext()?.Request.Query["activityId"].ToString() ?? "";
    string[] Roles => Context.User!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    async Task<AppUser> CurrentUser() {
        var user = await users.GetUserAsync(Context.User!);
        if (user is null || await users.IsLockedOutAsync(user) ||
            Context.User!.FindFirstValue(users.Options.ClaimsIdentity.SecurityStampClaimType) != await users.GetSecurityStampAsync(user) ||
            (!environment.IsDevelopment() && user.Id.StartsWith(DemoSessionSetup.Prefix))) {
            Context.Abort(); throw new HubException("A munkamenet lejárt. Jelentkezz be újra.");
        }
        return user;
    }
    public override async Task OnConnectedAsync() {
        await LoadComments();
        await Groups.AddToGroupAsync(Context.ConnectionId, ActivityId);
        await base.OnConnectedAsync();
    }
    public async Task<List<ActivityChat.CommentDto>> LoadComments() {
        var user = await CurrentUser();
        try { return await mediator.Send(new ActivityChat.List(ActivityId, user.Id, Roles), Context.ConnectionAborted); }
        catch (UnauthorizedAccessException e) { throw new HubException(e.Message); }
    }
    public async Task<ActivityChat.CommentDto> SendComment(string body) {
        var user = await CurrentUser();
        try {
            var comment = await mediator.Send(new ActivityChat.Send(ActivityId, user.Id, Roles, user.UserName ?? "Felhasználó", body), Context.ConnectionAborted);
            await Clients.Group(ActivityId).SendAsync("CommentAdded", ActivityId);
            return comment;
        }
        catch (Exception e) when (e is UnauthorizedAccessException or ArgumentException) { throw new HubException(e.Message); }
    }
}
