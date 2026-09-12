using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;
namespace Persistence;
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PractitionerProfile> Practitioners => Set<PractitionerProfile>();
    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
    public DbSet<AdmissionsOfficeProfile> AdmissionsOfficeProfiles => Set<AdmissionsOfficeProfile>();
    public DbSet<PatientActivity> PatientActivities => Set<PatientActivity>();
    public DbSet<ActivityPractitioner> ActivityPractitioners => Set<ActivityPractitioner>();
    public DbSet<PatientPractitionerAccess> PatientPractitionerAccesses => Set<PatientPractitionerAccess>();
    public DbSet<PractitionerBookingSettings> PractitionerBookingSettings => Set<PractitionerBookingSettings>();
    public DbSet<PractitionerWorkingHours> PractitionerWorkingHours => Set<PractitionerWorkingHours>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Patient>().HasIndex(x => x.TajNumber).IsUnique();
        b.Entity<PractitionerProfile>().HasIndex(x => x.TajNumber).IsUnique();
        b.Entity<Patient>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<PractitionerProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<AdminProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<AdmissionsOfficeProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<Patient>().HasOne<AppUser>().WithOne().HasForeignKey<Patient>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PractitionerProfile>().HasOne<AppUser>().WithOne().HasForeignKey<PractitionerProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AdminProfile>().HasOne<AppUser>().WithOne().HasForeignKey<AdminProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AdmissionsOfficeProfile>().HasOne<AppUser>().WithOne().HasForeignKey<AdmissionsOfficeProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PatientActivity>().HasKey(x => new { x.PatientId, x.ActivityId });
        b.Entity<PatientActivity>().HasOne(x => x.Patient).WithMany(x => x.PatientActivities).HasForeignKey(x => x.PatientId);
        b.Entity<PatientActivity>().HasOne(x => x.Activity).WithMany(x => x.PatientActivities).HasForeignKey(x => x.ActivityId);
        b.Entity<ActivityPractitioner>().HasKey(x => new { x.PractitionerId, x.ActivityId });
        b.Entity<ActivityPractitioner>().HasOne(x => x.Practitioner).WithMany(x => x.ActivityPractitioners).HasForeignKey(x => x.PractitionerId);
        b.Entity<ActivityPractitioner>().HasOne(x => x.Activity).WithMany(x => x.ActivityPractitioners).HasForeignKey(x => x.ActivityId);
        b.Entity<PatientPractitionerAccess>().HasKey(x => new { x.PatientId, x.PractitionerId });
        b.Entity<PatientPractitionerAccess>().HasOne(x => x.Patient).WithMany(x => x.PractitionerAccesses).HasForeignKey(x => x.PatientId);
        b.Entity<PatientPractitionerAccess>().HasOne(x => x.Practitioner).WithMany().HasForeignKey(x => x.PractitionerId);
        b.Entity<PractitionerBookingSettings>().HasOne(x => x.Practitioner).WithOne(x => x.BookingSettings).HasForeignKey<PractitionerBookingSettings>(x => x.PractitionerId);
        b.Entity<PractitionerWorkingHours>().HasOne(x => x.Practitioner).WithMany(x => x.WorkingHours).HasForeignKey(x => x.PractitionerId);
        b.Entity<PractitionerWorkingHours>().HasIndex(x => new { x.PractitionerId, x.DayOfWeek }).IsUnique();
        b.Entity<Appointment>().HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.Practitioner).WithMany().HasForeignKey(x => x.PractitionerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.Activity).WithOne().HasForeignKey<Appointment>(x => x.ActivityId).OnDelete(DeleteBehavior.Restrict);
        // A lemondott foglalás felszabadítja a helyet; Completed/NoShow továbbra is foglaltnak számít.
        b.Entity<Appointment>().HasIndex(x => new { x.PatientId, x.BookingDate }).IsUnique().HasFilter("\"Status\" <> 1");
        b.Entity<Appointment>().HasIndex(x => new { x.PractitionerId, x.StartTime }).IsUnique().HasFilter("\"Status\" <> 1");
        b.Entity<Appointment>().ToTable(t => {
            t.HasCheckConstraint("CK_Appointment_Date", "date(\"StartTime\") = \"BookingDate\"");
            t.HasCheckConstraint("CK_Appointment_Time", "strftime('%M:%S', \"StartTime\") = '00:00' AND time(\"StartTime\") >= '08:00:00' AND time(\"StartTime\") <= '19:00:00' AND datetime(\"EndTime\") = datetime(\"StartTime\", '+1 hour')");
        });
    }
}
