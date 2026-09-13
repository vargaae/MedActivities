using Microsoft.AspNetCore.Identity;
namespace Persistence.Identity;
public class AppUser : IdentityUser { }
public static class AppRoles
{
    public const string Admin = "Admin", AdmissionsOffice = "AdmissionsOffice", Patient = "Patient", Practitioner = "Practitioner";
    public static readonly string[] All = [Admin, AdmissionsOffice, Patient, Practitioner];
}