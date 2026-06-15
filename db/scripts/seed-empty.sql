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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163115_InitialCreate_Team'
)
BEGIN
    CREATE TABLE [Teams] (
        [Id] int NOT NULL,
        [Name] nvarchar(60) NOT NULL,
        [FifaRanking] int NOT NULL,
        [IsFictitious] bit NOT NULL,
        [Confederation] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163115_InitialCreate_Team'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Teams_Name] ON [Teams] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163115_InitialCreate_Team'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529163115_InitialCreate_Team', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604175903_AddUsersAndRoles'
)
BEGIN
    CREATE TABLE [Users] (
        [idUser] int NOT NULL IDENTITY,
        [name] nvarchar(100) NOT NULL,
        [lastname] nvarchar(100) NULL,
        [email] nvarchar(254) NOT NULL,
        [password] nvarchar(100) NOT NULL,
        [dateOfBirth] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([idUser])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604175903_AddUsersAndRoles'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([idUser]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604175903_AddUsersAndRoles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserRoles_UserId_Role] ON [UserRoles] ([UserId], [Role]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604175903_AddUsersAndRoles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_email] ON [Users] ([email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604175903_AddUsersAndRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260604175903_AddUsersAndRoles', N'8.0.28');
END;
GO

COMMIT;
GO

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
    VALUES (N'20260604201605_AddStadiumsSettingsAndAudit', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    ALTER TABLE [Teams] ADD [GroupName] nvarchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE TABLE [Groups] (
        [Name] nvarchar(3) NOT NULL,
        CONSTRAINT [PK_Groups] PRIMARY KEY ([Name])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE TABLE [Journeys] (
        [Number] int NOT NULL,
        CONSTRAINT [PK_Journeys] PRIMARY KEY ([Number])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE TABLE [Matches] (
        [MatchId] int NOT NULL,
        [MatchDate] datetime2 NOT NULL,
        [HomeTeamId] int NULL,
        [AwayTeamId] int NULL,
        [HomeGoals] int NOT NULL,
        [AwayGoals] int NOT NULL,
        [StadiumId] int NULL,
        [WasSimulated] bit NOT NULL,
        [MatchType] nvarchar(13) NOT NULL,
        [MatchCode] nvarchar(20) NULL,
        [Phase] nvarchar(20) NULL,
        [WonByPenalties] bit NULL,
        [HomePenaltyGoals] int NULL,
        [AwayPenaltyGoals] int NULL,
        [MatchHomeId] int NULL,
        [MatchAwayId] int NULL,
        [GroupName] nvarchar(3) NULL,
        [JourneyNumber] int NULL,
        CONSTRAINT [PK_Matches] PRIMARY KEY ([MatchId]),
        CONSTRAINT [FK_Matches_Groups_GroupName] FOREIGN KEY ([GroupName]) REFERENCES [Groups] ([Name]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Matches_Journeys_JourneyNumber] FOREIGN KEY ([JourneyNumber]) REFERENCES [Journeys] ([Number]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Matches_Matches_MatchAwayId] FOREIGN KEY ([MatchAwayId]) REFERENCES [Matches] ([MatchId]),
        CONSTRAINT [FK_Matches_Matches_MatchHomeId] FOREIGN KEY ([MatchHomeId]) REFERENCES [Matches] ([MatchId]),
        CONSTRAINT [FK_Matches_Stadiums_StadiumId] FOREIGN KEY ([StadiumId]) REFERENCES [Stadiums] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Matches_Teams_AwayTeamId] FOREIGN KEY ([AwayTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Matches_Teams_HomeTeamId] FOREIGN KEY ([HomeTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Teams_GroupName] ON [Teams] ([GroupName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_AwayTeamId] ON [Matches] ([AwayTeamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_GroupName] ON [Matches] ([GroupName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_HomeTeamId] ON [Matches] ([HomeTeamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_JourneyNumber] ON [Matches] ([JourneyNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_MatchAwayId] ON [Matches] ([MatchAwayId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_MatchHomeId] ON [Matches] ([MatchHomeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    CREATE INDEX [IX_Matches_StadiumId] ON [Matches] ([StadiumId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    ALTER TABLE [Teams] ADD CONSTRAINT [FK_Teams_Groups_GroupName] FOREIGN KEY ([GroupName]) REFERENCES [Groups] ([Name]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204001_AddFixtureAndMatches'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260605204001_AddFixtureAndMatches', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606210801_AddTeamFlagBase64'
)
BEGIN
    ALTER TABLE [Teams] ADD [FlagBase64] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606210801_AddTeamFlagBase64'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606210801_AddTeamFlagBase64', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223107_AddMatchIncidents'
)
BEGIN
    CREATE TABLE [MatchIncidents] (
        [Id] int NOT NULL,
        [Minute] int NOT NULL,
        [TeamId] int NOT NULL,
        [IncidentType] nvarchar(13) NOT NULL,
        [MatchId] int NULL,
        CONSTRAINT [PK_MatchIncidents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MatchIncidents_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([MatchId]) ON DELETE CASCADE,
        CONSTRAINT [FK_MatchIncidents_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223107_AddMatchIncidents'
)
BEGIN
    CREATE INDEX [IX_MatchIncidents_MatchId] ON [MatchIncidents] ([MatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223107_AddMatchIncidents'
)
BEGIN
    CREATE INDEX [IX_MatchIncidents_TeamId] ON [MatchIncidents] ([TeamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223107_AddMatchIncidents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606223107_AddMatchIncidents', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609183521_AddSelectedSimulationEngine'
)
BEGIN
    ALTER TABLE [TournamentSettings] ADD [SelectedSimulationEngine] nvarchar(60) NOT NULL DEFAULT N'Probabilístico';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609183521_AddSelectedSimulationEngine'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609183521_AddSelectedSimulationEngine', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610154413_AddNotifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL,
        [UserId] int NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([idUser]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610154413_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610154413_AddNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610154413_AddNotifications', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611134944_MakeNotificationIdIdentity'
)
BEGIN
    DROP TABLE [Notifications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611134944_MakeNotificationIdIdentity'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([idUser]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611134944_MakeNotificationIdIdentity'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611134944_MakeNotificationIdIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260611134944_MakeNotificationIdIdentity', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611232454_MakeAuditLogsGlobalWithActor'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'TournamentSettingsId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [TournamentSettingsId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611232454_MakeAuditLogsGlobalWithActor'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [PerformedByEmail] nvarchar(254) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260611232454_MakeAuditLogsGlobalWithActor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260611232454_MakeAuditLogsGlobalWithActor', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612121952_AddMatchRankingApplied'
)
BEGIN
    ALTER TABLE [Matches] ADD [RankingApplied] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612121952_AddMatchRankingApplied'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260612121952_AddMatchRankingApplied', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260613213813_AddMatchHasResult'
)
BEGIN
    ALTER TABLE [Matches] ADD [HasResult] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260613213813_AddMatchHasResult'
)
BEGIN
    UPDATE [Matches] SET [HasResult] = 1 WHERE [WasSimulated] = 1 OR ([HomeGoals] + [AwayGoals]) > 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260613213813_AddMatchHasResult'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260613213813_AddMatchHasResult', N'8.0.28');
END;
GO

COMMIT;
GO

