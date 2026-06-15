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
    VALUES (N'20260605204001_AddFixtureAndMatches', N'8.0.27');
END;
GO

COMMIT;
GO

