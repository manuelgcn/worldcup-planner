BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604201605_AddStadiumsSettingsAndAudit'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [Timestamp] datetime2 NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [Details] nvarchar(1000) NOT NULL,
        [TournamentSettingsId] int NOT NULL,
        [AffectedEntityType] nvarchar(100) NULL,
        [AffectedEntityId] int NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604201605_AddStadiumsSettingsAndAudit'
)
BEGIN
    CREATE TABLE [Stadiums] (
        [Id] int NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [City] nvarchar(60) NOT NULL,
        [Description] nvarchar(400) NULL,
        [Capacity] int NOT NULL,
        CONSTRAINT [PK_Stadiums] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604201605_AddStadiumsSettingsAndAudit'
)
BEGIN
    CREATE TABLE [TournamentSettings] (
        [Id] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [MaxMatchesPerDay] int NOT NULL,
        [DaysBetweenJourneys] int NOT NULL,
        [FixtureSeed] int NOT NULL,
        [CrossingsSeed] int NOT NULL,
        [SimulationSeed] int NOT NULL,
        [CompletionSeed] int NOT NULL,
        CONSTRAINT [PK_TournamentSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604201605_AddStadiumsSettingsAndAudit'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Stadiums_Name] ON [Stadiums] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604201605_AddStadiumsSettingsAndAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260604201605_AddStadiumsSettingsAndAudit', N'8.0.27');
END;
GO

COMMIT;
GO

