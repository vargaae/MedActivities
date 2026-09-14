using Application.Activities.Queries;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Application.Comments;

public static class ActivityChat
{
    public record CommentDto(string Id, string UserId, string DisplayName, string Body, DateTime CreatedAt);
    public record List(string ActivityId, string UserId, string[] Roles) : IRequest<List<CommentDto>>;
    public record Send(string ActivityId, string UserId, string[] Roles, string DisplayName, string Body) : IRequest<CommentDto>;
    public class ListHandler(AppDbContext db) : IRequestHandler<List, List<CommentDto>>
    {
        public async Task<List<CommentDto>> Handle(List request, CancellationToken ct) {
            if (!await ActivityVisibility.For(db, request.UserId, request.Roles).AnyAsync(a => a.Id == request.ActivityId, ct))
                throw new UnauthorizedAccessException("Az esemény nem érhető el.");
            var items = await db.ActivityComments.Where(c => c.ActivityId == request.ActivityId)
                .OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id).Take(200)
                .Select(c => new CommentDto(c.Id, c.UserId, c.DisplayName, c.Body, c.CreatedAt)).ToListAsync(ct);
            items.Reverse(); return items;
        }
    }
    public class SendHandler(AppDbContext db) : IRequestHandler<Send, CommentDto>
    {
        public async Task<CommentDto> Handle(Send request, CancellationToken ct) {
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 2000)
                throw new ArgumentException("Az üzenet 1–2000 karakter lehet.");
            if (!await ActivityVisibility.For(db, request.UserId, request.Roles).AnyAsync(a => a.Id == request.ActivityId, ct))
                throw new UnauthorizedAccessException("Az esemény nem érhető el.");
            var comment = new ActivityComment { ActivityId = request.ActivityId, UserId = request.UserId,
                DisplayName = request.DisplayName, Body = request.Body.Trim() };
            db.ActivityComments.Add(comment); await db.SaveChangesAsync(ct);
            return new(comment.Id, comment.UserId, comment.DisplayName, comment.Body, comment.CreatedAt);
        }
    }
}
