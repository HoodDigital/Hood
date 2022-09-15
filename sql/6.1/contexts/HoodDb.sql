BEGIN TRANSACTION;
GO

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
GO

CREATE TABLE [HoodOptions] (
    [Id] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodOptions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [UserProfiles] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [Anonymous] bit NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [DisplayName] nvarchar(max) NULL,
    [BillingAddressJson] nvarchar(max) NULL,
    [DeliveryAddressJson] nvarchar(max) NULL,
    [AvatarJson] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([Id])
);
GO

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
GO

CREATE TABLE [ApplicationUser] (
    [Id] nvarchar(450) NOT NULL,
    [UserProfileId] nvarchar(450) NULL,
    [Active] bit NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [LastLogOn] datetime2 NOT NULL,
    [LastLoginIP] nvarchar(max) NULL,
    [LastLoginLocation] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [NormalizedUserName] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [NormalizedEmail] nvarchar(max) NULL,
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
    CONSTRAINT [PK_ApplicationUser] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApplicationUser_UserProfiles_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [UserProfiles] ([Id])
);
GO

CREATE TABLE [HoodLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [Time] datetime2 NOT NULL,
    [Title] nvarchar(max) NULL,
    [Detail] nvarchar(max) NULL,
    [Type] int NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Source] nvarchar(max) NULL,
    [SourceUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodLogs_ApplicationUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [ApplicationUser] ([Id])
);
GO

CREATE INDEX [IX_ApplicationUser_UserProfileId] ON [ApplicationUser] ([UserProfileId]);
GO

CREATE INDEX [IX_HoodLogs_UserId] ON [HoodLogs] ([UserId]);
GO

CREATE INDEX [IX_HoodMedia_DirectoryId] ON [HoodMedia] ([DirectoryId]);
GO

CREATE INDEX [IX_HoodMediaDirectories_ParentId] ON [HoodMediaDirectories] ([ParentId]);
GO

COMMIT;
GO

