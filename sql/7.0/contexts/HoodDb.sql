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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE TABLE [HoodLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Time] datetime2 NOT NULL,
        [Title] nvarchar(max) NULL,
        [Detail] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [UserId] nvarchar(max) NULL,
        [Source] nvarchar(max) NULL,
        [SourceUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_HoodLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE TABLE [HoodMediaDirectories] (
        [Id] int NOT NULL IDENTITY,
        [DisplayName] nvarchar(max) NULL,
        [Slug] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [OwnerId] nvarchar(max) NULL,
        [ParentId] int NULL,
        CONSTRAINT [PK_HoodMediaDirectories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HoodMediaDirectories_HoodMediaDirectories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [HoodMediaDirectories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE TABLE [HoodOptions] (
        [Id] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_HoodOptions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE TABLE [HoodMedia] (
        [Id] int NOT NULL IDENTITY,
        [DirectoryId] int NULL,
        [Directory] nvarchar(max) NULL,
        [FileSize] bigint NOT NULL,
        [FileType] nvarchar(max) NULL,
        [Filename] nvarchar(max) NULL,
        [BlobReference] nvarchar(max) NULL,
        [Url] nvarchar(max) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ThumbUrl] nvarchar(max) NULL,
        [SmallUrl] nvarchar(max) NULL,
        [MediumUrl] nvarchar(max) NULL,
        [LargeUrl] nvarchar(max) NULL,
        [UniqueId] nvarchar(max) NULL,
        [GenericFileType] int NOT NULL,
        CONSTRAINT [PK_HoodMedia] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HoodMedia_HoodMediaDirectories_DirectoryId] FOREIGN KEY ([DirectoryId]) REFERENCES [HoodMediaDirectories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE INDEX [IX_HoodMedia_DirectoryId] ON [HoodMedia] ([DirectoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    CREATE INDEX [IX_HoodMediaDirectories_ParentId] ON [HoodMediaDirectories] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602173244_v7_baseline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260602173244_v7_baseline', N'10.0.8');
END;

COMMIT;
GO

