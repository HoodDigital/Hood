-- Apply key 07.00.00/20 | Hood v7.0.0 - HoodDb context. Embedded; applied by hood-schema (DbUp) in LogicalName order.
-- HoodDb tables — idempotent: the whole block runs only on a database that doesn't
-- already have it. DbUp journals this script; no EF migration-history table is used.
IF OBJECT_ID(N'[HoodOptions]', 'U') IS NULL
BEGIN
    CREATE TABLE [HoodLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Time] datetime2 NOT NULL,
        [Title] nvarchar(max) NULL,
        [Detail] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        [Source] nvarchar(max) NULL,
        [SourceUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_HoodLogs] PRIMARY KEY ([Id])
    );

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

    CREATE TABLE [HoodOptions] (
        [Id] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_HoodOptions] PRIMARY KEY ([Id])
    );

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

    CREATE INDEX [IX_HoodMedia_DirectoryId] ON [HoodMedia] ([DirectoryId]);

    CREATE INDEX [IX_HoodMediaDirectories_ParentId] ON [HoodMediaDirectories] ([ParentId]);

    CREATE INDEX [IX_HoodLogs_UserId] ON [HoodLogs] ([UserId]);
END;
