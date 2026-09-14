using Application.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Application.Activities.Queries;
public class GetActivityList
{
    public record Query(string UserId, string[] Roles, string? PatientId = null, string? PractitionerId = null) : IRequest<List<ActivityDto>>;
    public class Handler(AppDbContext context) : IRequestHandler<Query,List<ActivityDto>>
    {
        public async Task<List<ActivityDto>> Handle(Query request,CancellationToken cancellationToken)
            => await ActivityVisibility.For(context, request.UserId, request.Roles)
                .Where(a => request.PatientId == null || a.PatientActivities.Any(p => p.PatientId == request.PatientId))
                .Where(a => request.PractitionerId == null || a.ActivityPractitioners.Any(p => p.PractitionerId == request.PractitionerId))
                .AsNoTracking().OrderBy(a=>a.Date).ThenBy(a=>a.Id)
                .Select(ActivityDto.Projection).ToListAsync(cancellationToken);
    }
}
