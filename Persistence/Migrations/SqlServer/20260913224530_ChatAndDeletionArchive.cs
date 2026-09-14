using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ChatAndDeletionArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ActivityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityComments_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeletedRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RecordKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityComments_ActivityId_CreatedAt",
                table: "ActivityComments",
                columns: new[] { "ActivityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedRecords_TableName_DeletedAtUtc",
                table: "DeletedRecords",
                columns: new[] { "TableName", "DeletedAtUtc" });
            foreach (var table in ArchivedTables)
            {
                var key = table switch {
                    "PatientActivities" => "d.[PatientId], d.[ActivityId]",
                    "ActivityPractitioners" => "d.[PractitionerId], d.[ActivityId]",
                    "PatientPractitionerAccesses" => "d.[PatientId], d.[PractitionerId]",
                    _ => "d.[Id]"
                };
                migrationBuilder.Sql($"""
                    CREATE TRIGGER [TR_{table}_ArchiveDelete] ON [{table}]
                    AFTER DELETE AS
                    BEGIN
                        SET NOCOUNT ON;
                        INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
                        SELECT N'{table}',
                            (SELECT {key} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                            (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
                            SYSUTCDATETIME(),
                            COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
                        FROM deleted d;
                    END
                    """);
            }
            migrationBuilder.Sql("""
                CREATE TRIGGER [TR_DeletedRecords_Immutable] ON [DeletedRecords]
                INSTEAD OF UPDATE, DELETE AS
                BEGIN
                    THROW 51001, N'A törlési archívum nem módosítható vagy törölhető alkalmazásművelettel.', 1;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Never destroy medical snapshots during an automatic rollback.
            throw new NotSupportedException("Az archív migráció automatikusan nem vonható vissza. Előbb készíts teljes mentést és tervezett adatmegőrző migrációt.");
        }
        private static readonly string[] ArchivedTables = [
            "Patients", "Practitioners", "Activities", "Appointments", "PatientActivities", "ActivityPractitioners",
            "PatientPractitionerAccesses", "PractitionerBookingSettings", "PractitionerWorkingHours",
            "PatientNotes", "PatientDocuments", "AdminProfiles", "AdmissionsOfficeProfiles", "ActivityComments"
        ];
    }
}
