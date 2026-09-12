using Domain;
using MediatR;
using Persistence;
namespace Application.Activities.Commands;
public class EditActivity
{
    public class Command : IRequest { public required Activity Activity { get; set; } }
    public class Handler(AppDbContext context) : IRequestHandler<Command>
    {
        public async Task Handle(Command request,CancellationToken cancellationToken)
        {
            var target=await context.Activities.FindAsync([request.Activity.Id],cancellationToken)
                ?? throw new Exception("Activity not found");
            var source=request.Activity;
            // A meglévő űrlap mezői. Kapcsolatok, orvosi megjegyzések és státusz megmaradnak.
            target.Title=source.Title;target.Date=source.Date;target.Description=source.Description;
            target.Category=source.Category;target.City=source.City;target.Venue=source.Venue;
            target.Latitude=source.Latitude;target.Longitude=source.Longitude;
            target.UpdatedAt=DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}