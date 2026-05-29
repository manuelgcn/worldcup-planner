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
    VALUES (N'20260529163115_InitialCreate_Team', N'8.0.27');
END;
GO

COMMIT;
GO

