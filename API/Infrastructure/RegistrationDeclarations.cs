using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Med;

public class DeclarationAcceptance
{
    public bool Accepted { get; set; }
    [MaxLength(80)] public string Version { get; set; } = "";
    [MaxLength(100)] public string SignedName { get; set; } = "";
}

public record RegistrationDeclaration(string Version, string Title, string Highlight, string Text, bool RequiresSignature)
{
    public string ClaimType => "medactivities:declaration:" + Version;
}

// Published versions are immutable. Text changes require a new version; retain old versions in source control.
public static class RegistrationDeclarations
{
    public static readonly RegistrationDeclaration Patient = new(
        "patient-2026-10-01-v1", "Páciens adatkezelési nyilatkozata",
        "Az általam megosztott és feltöltött egészségügyi adatokat, dokumentumokat és leleteket az ellátásomban részt vevő, hozzáférésre jogosult kezelőorvosok a vizsgálatomhoz és kezelésemhez felhasználhatják.",
        "Tudomásul veszem, hogy a rendszerben megadott személyes és egészségügyi adataim az ellátásom megszervezését, dokumentálását és az ellátásban részt vevők közötti szükséges információcserét szolgálják. Ez nem jelent korlátlan hozzáférést minden kezelőorvosnak, és nem ad engedélyt reklámcélú, nyilvános vagy az ellátástól független felhasználásra. A felvételi iroda kizárólag a feladatához szükséges adatokhoz férhet hozzá. Az adatkezelőnél tájékoztatást és hozzáférést, helyesbítést, valamint a jogszabályi feltételek mellett törlést vagy korlátozást kérhetek, és a NAIH-hoz panasszal fordulhatok. Amennyiben valamely adatkezelés hozzájáruláson alapul, azt visszavonhatom; ez nem érinti a korábbi adatkezelés jogszerűségét vagy a jogszabály által előírt megőrzést. Ez a nyilatkozat a tájékoztatás megismerését rögzíti, nem helyettesíti az adatkezelési jogalap meghatározását vagy az orvosi beavatkozáshoz szükséges beleegyezést.", false);

    public static readonly RegistrationDeclaration Staff = new(
        "confidentiality-2026-10-01-v1", "Dolgozói titoktartási nyilatkozat",
        "A páciens bizalmas adatait és leleteit kizárólag az ellátásához szükséges mértékben, az ellátásában részt vevő, jogosult kezelőorvossal osztom meg. Egyébként az információkat titokban tartom.",
        "Kezelőorvosként vagy a felvételi iroda munkatársaként kizárólag a feladatomhoz szükséges adatokat tekintem meg és használom fel. Illetéktelen személynek nem adok hozzáférést, adatot nem teszek közzé, és nem másolok saját vagy az ellátástól független célra. A felvételi irodai hozzáférés nem jogosít orvosi döntéshozatalra. A titoktartás a munkavégzés és a hozzáférésem megszűnését követően is fennmarad. A jogszabály által előírt adatszolgáltatási kötelezettségeket a szükséges mértékre korlátozva teljesítem; adatvédelmi incidenst az intézmény kijelölt felelősének jelzek. Teljes nevem megadásával és a jelölőnégyzet bejelölésével személyesen teszem meg ezt a nyilatkozatot. Ez naplózott elektronikus elfogadás, nem minősített elektronikus aláírás.", true);

    public static RegistrationDeclaration? ForRoles(IEnumerable<string> roles) =>
        roles.Any(r => r is "Practitioner" or "AdmissionsOffice") ? Staff :
        roles.Contains("Patient") ? Patient : null;

    public static string? Validate(RegistrationDeclaration policy, DeclarationAcceptance? acceptance) =>
        acceptance?.Accepted != true ? "A nyilatkozat kifejezett elfogadása kötelező." :
        acceptance.Version != policy.Version ? "A nyilatkozat verziója megváltozott. Töltsd be újra, és olvasd el az aktuális változatot." :
        policy.RequiresSignature && (string.IsNullOrWhiteSpace(acceptance.SignedName) || acceptance.SignedName.Trim().Length < 3)
            ? "A titoktartási nyilatkozathoz add meg a teljes nevedet." : null;

    public static Task<bool> HasAccepted(AppDbContext db, string userId, RegistrationDeclaration policy) =>
        db.UserClaims.AnyAsync(c => c.UserId == userId && c.ClaimType == policy.ClaimType);

    public static DateTimeOffset Record(AppDbContext db, string userId, RegistrationDeclaration policy, string signedName, IEnumerable<string> roles)
    {
        var acceptedAtUtc = DateTimeOffset.UtcNow;
        // Existing Identity table: no schema migration; never accept timestamps or policy text from the client.
        db.UserClaims.Add(new IdentityUserClaim<string>
        {
            UserId = userId,
            ClaimType = policy.ClaimType,
            ClaimValue = JsonSerializer.Serialize(new
            {
                policy.Version, AcceptedAtUtc = acceptedAtUtc,
                SignedName = signedName.Trim(), Roles = roles.ToArray(),
                TextSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(policy.Title + "\n" + policy.Highlight + "\n" + policy.Text)))
            })
        });
        return acceptedAtUtc;
    }
}
