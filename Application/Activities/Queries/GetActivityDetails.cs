using Application.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Application.Activities.Queries;
public class GetActivityDetails
{
    public class Query : IRequest<ActivityDto?> { public required string Id { get; set; } }
    public class Handler(AppDbContext context) : IRequestHandler<Query,ActivityDto?>
    {
        public async Task<ActivityDto?> Handle(Query request,CancellationToken cancellationToken)
            => await context.Activities.AsNoTracking().Where(a=>a.Id==request.Id)
                .Select(ActivityDto.Projection).SingleOrDefaultAsync(cancellationToken);
    }
}