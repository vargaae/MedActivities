IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Activities] (
    [Id] nvarchar(128) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Date] datetime2 NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [IsCancelled] bit NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [Venue] nvarchar(max) NOT NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [MedicalNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] nvarchar(128) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedByUserId] nvarchar(128) NULL,
    CONSTRAINT [PK_Activities] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(128) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(128) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AdminProfiles] (
    [Id] nvarchar(128) NOT NULL,
    [UserId] nvarchar(128) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AdminProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdminProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AdmissionsOfficeProfiles] (
    [Id] nvarchar(128) NOT NULL,
    [UserId] nvarchar(128) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AdmissionsOfficeProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdmissionsOfficeProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(128) NOT NULL,
    [RoleId] nvarchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(128) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Patients] (
    [Id] nvarchar(128) NOT NULL,
    [TajNumber] nvarchar(9) NOT NULL,
    [UserId] nvarchar(128) NULL,
    [Name] nvarchar(100) NOT NULL,
    [BirthDate] date NOT NULL,
    [Email] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Patients_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Practitioners] (
    [Id] nvarchar(128) NOT NULL,
    [UserId] nvarchar(128) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [TajNumber] nvarchar(9) NOT NULL,
    [Specialty] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [Venue] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Practitioners] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Practitioners_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PatientActivities] (
    [PatientId] nvarchar(128) NOT NULL,
    [ActivityId] nvarchar(128) NOT NULL,
    CONSTRAINT [PK_PatientActivities] PRIMARY KEY ([PatientId], [ActivityId]),
    CONSTRAINT [FK_PatientActivities_Activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PatientActivities_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PatientDocuments] (
    [Id] nvarchar(128) NOT NULL,
    [PatientId] nvarchar(128) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [Content] varbinary(max) NOT NULL,
    [UploadedByUserId] nvarchar(128) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PatientDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PatientDocuments_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PatientDocuments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PatientNotes] (
    [Id] nvarchar(128) NOT NULL,
    [PatientId] nvarchar(128) NOT NULL,
    [Text] nvarchar(2000) NOT NULL,
    [AuthorUserId] nvarchar(128) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PatientNotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PatientNotes_AspNetUsers_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PatientNotes_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ActivityPractitioners] (
    [PractitionerId] nvarchar(128) NOT NULL,
    [ActivityId] nvarchar(128) NOT NULL,
    CONSTRAINT [PK_ActivityPractitioners] PRIMARY KEY ([PractitionerId], [ActivityId]),
    CONSTRAINT [FK_ActivityPractitioners_Activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ActivityPractitioners_Practitioners_PractitionerId] FOREIGN KEY ([PractitionerId]) REFERENCES [Practitioners] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Appointments] (
    [Id] nvarchar(128) NOT NULL,
    [PatientId] nvarchar(128) NOT NULL,
    [PractitionerId] nvarchar(128) NOT NULL,
    [ActivityId] nvarchar(128) NOT NULL,
    [BookingDate] date NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Note] nvarchar(max) NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Appointment_Date] CHECK (CONVERT(date, [StartTime]) = [BookingDate]),
    CONSTRAINT [CK_Appointment_Time] CHECK (DATEPART(MINUTE, [StartTime]) = 0 AND DATEPART(SECOND, [StartTime]) = 0 AND DATEPART(NANOSECOND, [StartTime]) = 0 AND CONVERT(time, [StartTime]) >= '08:00:00' AND CONVERT(time, [StartTime]) <= '19:00:00' AND [EndTime] = DATEADD(HOUR, 1, [StartTime])),
    CONSTRAINT [FK_Appointments_Activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Practitioners_PractitionerId] FOREIGN KEY ([PractitionerId]) REFERENCES [Practitioners] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PatientPractitionerAccesses] (
    [PatientId] nvarchar(128) NOT NULL,
    [PractitionerId] nvarchar(128) NOT NULL,
    [GrantedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PatientPractitionerAccesses] PRIMARY KEY ([PatientId], [PractitionerId]),
    CONSTRAINT [FK_PatientPractitionerAccesses_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PatientPractitionerAccesses_Practitioners_PractitionerId] FOREIGN KEY ([PractitionerId]) REFERENCES [Practitioners] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PractitionerBookingSettings] (
    [Id] nvarchar(128) NOT NULL,
    [PractitionerId] nvarchar(128) NOT NULL,
    [BookingEnabled] bit NOT NULL,
    CONSTRAINT [PK_PractitionerBookingSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PractitionerBookingSettings_Practitioners_PractitionerId] FOREIGN KEY ([PractitionerId]) REFERENCES [Practitioners] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PractitionerWorkingHours] (
    [Id] nvarchar(128) NOT NULL,
    [PractitionerId] nvarchar(128) NOT NULL,
    [DayOfWeek] int NOT NULL,
    [IsWorkingDay] bit NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    CONSTRAINT [PK_PractitionerWorkingHours] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PractitionerWorkingHours_Practitioners_PractitionerId] FOREIGN KEY ([PractitionerId]) REFERENCES [Practitioners] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_ActivityPractitioners_ActivityId] ON [ActivityPractitioners] ([ActivityId]);

CREATE UNIQUE INDEX [IX_AdminProfiles_UserId] ON [AdminProfiles] ([UserId]);

CREATE UNIQUE INDEX [IX_AdmissionsOfficeProfiles_UserId] ON [AdmissionsOfficeProfiles] ([UserId]);

CREATE UNIQUE INDEX [IX_Appointments_ActivityId] ON [Appointments] ([ActivityId]);

CREATE UNIQUE INDEX [IX_Appointments_PatientId_BookingDate] ON [Appointments] ([PatientId], [BookingDate]) WHERE [Status] <> 1;

CREATE UNIQUE INDEX [IX_Appointments_PractitionerId_StartTime] ON [Appointments] ([PractitionerId], [StartTime]) WHERE [Status] <> 1;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_PatientActivities_ActivityId] ON [PatientActivities] ([ActivityId]);

CREATE INDEX [IX_PatientDocuments_PatientId] ON [PatientDocuments] ([PatientId]);

CREATE INDEX [IX_PatientDocuments_UploadedByUserId] ON [PatientDocuments] ([UploadedByUserId]);

CREATE INDEX [IX_PatientNotes_AuthorUserId] ON [PatientNotes] ([AuthorUserId]);

CREATE INDEX [IX_PatientNotes_PatientId] ON [PatientNotes] ([PatientId]);

CREATE INDEX [IX_PatientPractitionerAccesses_PractitionerId] ON [PatientPractitionerAccesses] ([PractitionerId]);

CREATE UNIQUE INDEX [IX_Patients_TajNumber] ON [Patients] ([TajNumber]);

CREATE UNIQUE INDEX [IX_Patients_UserId] ON [Patients] ([UserId]) WHERE [UserId] IS NOT NULL;

CREATE UNIQUE INDEX [IX_PractitionerBookingSettings_PractitionerId] ON [PractitionerBookingSettings] ([PractitionerId]);

CREATE UNIQUE INDEX [IX_Practitioners_TajNumber] ON [Practitioners] ([TajNumber]);

CREATE UNIQUE INDEX [IX_Practitioners_UserId] ON [Practitioners] ([UserId]);

CREATE UNIQUE INDEX [IX_PractitionerWorkingHours_PractitionerId_DayOfWeek] ON [PractitionerWorkingHours] ([PractitionerId], [DayOfWeek]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260913164236_SqlServerInitial', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [ActivityComments] (
    [Id] nvarchar(128) NOT NULL,
    [ActivityId] nvarchar(128) NOT NULL,
    [UserId] nvarchar(128) NOT NULL,
    [DisplayName] nvarchar(256) NOT NULL,
    [Body] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ActivityComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ActivityComments_Activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DeletedRecords] (
    [Id] bigint NOT NULL IDENTITY,
    [TableName] nvarchar(128) NOT NULL,
    [RecordKey] nvarchar(512) NOT NULL,
    [SnapshotJson] nvarchar(max) NOT NULL,
    [DeletedAtUtc] datetime2 NOT NULL,
    [DeletedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_DeletedRecords] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_ActivityComments_ActivityId_CreatedAt] ON [ActivityComments] ([ActivityId], [CreatedAt]);

CREATE INDEX [IX_DeletedRecords_TableName_DeletedAtUtc] ON [DeletedRecords] ([TableName], [DeletedAtUtc]);

GO
CREATE TRIGGER [TR_Patients_ArchiveDelete] ON [Patients]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'Patients',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_Practitioners_ArchiveDelete] ON [Practitioners]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'Practitioners',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_Activities_ArchiveDelete] ON [Activities]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'Activities',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_Appointments_ArchiveDelete] ON [Appointments]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'Appointments',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PatientActivities_ArchiveDelete] ON [PatientActivities]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PatientActivities',
        (SELECT d.[PatientId], d.[ActivityId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_ActivityPractitioners_ArchiveDelete] ON [ActivityPractitioners]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'ActivityPractitioners',
        (SELECT d.[PractitionerId], d.[ActivityId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PatientPractitionerAccesses_ArchiveDelete] ON [PatientPractitionerAccesses]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PatientPractitionerAccesses',
        (SELECT d.[PatientId], d.[PractitionerId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PractitionerBookingSettings_ArchiveDelete] ON [PractitionerBookingSettings]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PractitionerBookingSettings',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PractitionerWorkingHours_ArchiveDelete] ON [PractitionerWorkingHours]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PractitionerWorkingHours',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PatientNotes_ArchiveDelete] ON [PatientNotes]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PatientNotes',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_PatientDocuments_ArchiveDelete] ON [PatientDocuments]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'PatientDocuments',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_AdminProfiles_ArchiveDelete] ON [AdminProfiles]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'AdminProfiles',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_AdmissionsOfficeProfiles_ArchiveDelete] ON [AdmissionsOfficeProfiles]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'AdmissionsOfficeProfiles',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_ActivityComments_ArchiveDelete] ON [ActivityComments]
AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [DeletedRecords] ([TableName], [RecordKey], [SnapshotJson], [DeletedAtUtc], [DeletedBy])
    SELECT N'ActivityComments',
        (SELECT d.[Id] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT d.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER),
        SYSUTCDATETIME(),
        COALESCE(CONVERT(nvarchar(256), SESSION_CONTEXT(N'MedActivitiesUserId')), ORIGINAL_LOGIN())
    FROM deleted d;
END
GO

GO
CREATE TRIGGER [TR_DeletedRecords_Immutable] ON [DeletedRecords]
INSTEAD OF UPDATE, DELETE AS
BEGIN
    THROW 51001, N'A törlési archívum nem módosítható vagy törölhető alkalmazásművelettel.', 1;
END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260913224530_ChatAndDeletionArchive', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Patients] ADD [BirthPlace] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915005633_AddPatientBirthPlace', N'10.0.3');

COMMIT;
GO

