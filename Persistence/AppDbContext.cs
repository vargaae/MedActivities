using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;
namespace Persistence;
public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
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
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
    public DbSet<PatientNote> PatientNotes => Set<PatientNote>();
    public DbSet<ActivityComment> ActivityComments => Set<ActivityComment>();
    public DbSet<DeletedRecord> DeletedRecords => Set<DeletedRecord>();
    public string? AuditUserId { get; set; }
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ActivityComment>().HasOne(c => c.Activity).WithMany().HasForeignKey(c => c.ActivityId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ActivityComment>().Property(c => c.Body).HasMaxLength(2000);
        b.Entity<ActivityComment>().Property(c => c.DisplayName).HasMaxLength(256);
        b.Entity<ActivityComment>().HasIndex(c => new { c.ActivityId, c.CreatedAt });
        b.Entity<DeletedRecord>().Property(d => d.TableName).HasMaxLength(128);
        b.Entity<DeletedRecord>().Property(d => d.RecordKey).HasMaxLength(512);
        b.Entity<DeletedRecord>().Property(d => d.DeletedBy).HasMaxLength(256);
        b.Entity<DeletedRecord>().HasIndex(d => new { d.TableName, d.DeletedAtUtc });
        b.Entity<PatientDocument>().HasOne(d => d.Patient).WithMany().HasForeignKey(d => d.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PatientDocument>().HasOne<AppUser>().WithMany().HasForeignKey(d => d.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PatientDocument>().Property(d => d.Title).HasMaxLength(200);
        b.Entity<PatientDocument>().Property(d => d.FileName).HasMaxLength(255);
        b.Entity<PatientDocument>().Property(d => d.ContentType).HasMaxLength(100);
        b.Entity<PatientNote>().HasOne(n => n.Patient).WithMany().HasForeignKey(n => n.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PatientNote>().HasOne<AppUser>().WithMany().HasForeignKey(n => n.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PatientNote>().Property(n => n.Text).HasMaxLength(2000);
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
        var sqlServer = Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer";
        b.Entity<Appointment>().HasIndex(x => new { x.PatientId, x.BookingDate }).IsUnique().HasFilter(sqlServer ? "[Status] <> 1" : "\"Status\" <> 1");
        b.Entity<Appointment>().HasIndex(x => new { x.PractitionerId, x.StartTime }).IsUnique().HasFilter(sqlServer ? "[Status] <> 1" : "\"Status\" <> 1");
        b.Entity<Appointment>().ToTable(t => {
            t.HasCheckConstraint("CK_Appointment_Date", sqlServer
                ? "CONVERT(date, [StartTime]) = [BookingDate]" : "date(\"StartTime\") = \"BookingDate\"");
            t.HasCheckConstraint("CK_Appointment_Time", sqlServer
                ? "DATEPART(MINUTE, [StartTime]) = 0 AND DATEPART(SECOND, [StartTime]) = 0 AND DATEPART(NANOSECOND, [StartTime]) = 0 AND CONVERT(time, [StartTime]) >= '08:00:00' AND CONVERT(time, [StartTime]) <= '19:00:00' AND [EndTime] = DATEADD(HOUR, 1, [StartTime])"
                : "strftime('%M:%S', \"StartTime\") = '00:00' AND time(\"StartTime\") >= '08:00:00' AND time(\"StartTime\") <= '19:00:00' AND datetime(\"EndTime\") = datetime(\"StartTime\", '+1 hour')");
        });
        if (sqlServer)
        {
            // A többszörös összetett kulcsok is a SQL Server indexméret-határa alatt maradnak.
            foreach (var property in b.Model.GetEntityTypes().SelectMany(e => e.GetProperties())
                .Where(p => p.ClrType == typeof(string) && (p.Name == "Id" || p.Name.EndsWith("Id"))))
                property.SetMaxLength(128);
            b.Entity<Patient>().Property(p => p.TajNumber).HasMaxLength(9);
            b.Entity<PractitionerProfile>().Property(p => p.TajNumber).HasMaxLength(9);
            b.Entity<Patient>().Property(p => p.Name).HasMaxLength(100);
            b.Entity<Activity>().Property(a => a.Title).HasMaxLength(200);
        }
    }
}
