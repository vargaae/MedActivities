using Application.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Queries;

public class GetActivityPage
{
    public record Query(string UserId, string[] Roles, int Page, string? PatientId, string? PractitionerId,
        string? Category, DateTime? From, DateTime? To) : IRequest<Result>;
    public record Result(List<ActivityDto> Items, int? NextPage);
    public class Handler(AppDbContext context) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(Query request, CancellationToken ct)
        {
            var rows = await ActivityVisibility.For(context, request.UserId, request.Roles)
                .AsNoTracking().AsSplitQuery()
                .Where(a => request.PatientId == null || a.PatientActivities.Any(p => p.PatientId == request.PatientId))
                .Where(a => request.PractitionerId == null || a.ActivityPractitioners.Any(p => p.PractitionerId == request.PractitionerId))
                .Where(a => request.Category == null || a.Category == request.Category)
                .Where(a => request.From == null || a.Date >= request.From)
                .Where(a => request.To == null || a.Date < request.To)
                .OrderByDescending(a => a.Date).ThenByDescending(a => a.Id)
                .Skip((request.Page - 1) * 30).Take(31)
                .Select(ActivityDto.Projection).ToListAsync(ct);
            return new(rows.Take(30).ToList(), rows.Count > 30 ? request.Page + 1 : null);
        }
    }
}
