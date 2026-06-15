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
    VALUES (N'20260604175903_AddUsersAndRoles', N'8.0.27');
END;
GO

COMMIT;
GO

