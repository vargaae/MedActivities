using Application.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Queries;

public class GetActivityPage
{
    public record Query(string UserId, string[] Roles, int Page, string? PatientId, string? PractitionerId,
        string? Category, DateTime? From, DateTime? To, int PageSize = 30) : IRequest<Result>;
    public record Result(List<ActivityDto> Items, int? NextPage, int TotalCount, int Page, int PageSize);
    public class Handler(AppDbContext context) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(Query request, CancellationToken ct)
        {
            var query = ActivityVisibility.For(context, request.UserId, request.Roles)
                .AsNoTracking().AsSplitQuery()
                .Where(a => request.PatientId == null || a.PatientActivities.Any(p => p.PatientId == request.PatientId))
                .Where(a => request.PractitionerId == null || a.ActivityPractitioners.Any(p => p.PractitionerId == request.PractitionerId))
                .Where(a => request.Category == null || a.Category == request.Category)
                .Where(a => request.From == null || a.Date >= request.From)
                .Where(a => request.To == null || a.Date < request.To);
            var total = await query.CountAsync(ct);
            var page = Math.Min(request.Page, Math.Max(1, (int)Math.Ceiling((double)total / request.PageSize)));
            var rows = await query
                .OrderByDescending(a => a.Date).ThenByDescending(a => a.Id)
                .Skip((page - 1) * request.PageSize).Take(request.PageSize)
                .Select(ActivityDto.Projection).ToListAsync(ct);
            return new(rows, page * request.PageSize < total ? page + 1 : null, total, page, request.PageSize);
        }
    }
}
