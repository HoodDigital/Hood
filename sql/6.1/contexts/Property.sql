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

CREATE TABLE [HoodProperties] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NULL,
    [Reference] nvarchar(max) NULL,
    [Tags] nvarchar(max) NULL,
    [ContactName] nvarchar(max) NULL,
    [Number] nvarchar(max) NULL,
    [Address1] nvarchar(max) NULL,
    [Address2] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [County] nvarchar(max) NULL,
    [Postcode] nvarchar(max) NULL,
    [Country] nvarchar(max) NULL,
    [Latitude] float NOT NULL DEFAULT (0.0),
    [Longitude] float NOT NULL DEFAULT (0.0),
    [Status] int NOT NULL,
    [PublishDate] datetime2 NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [SystemNotes] nvarchar(max) NULL,
    [AllowComments] bit NOT NULL,
    [Public] bit NOT NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [InfoDownloadJson] nvarchar(max) NULL,
    [ListingType] nvarchar(max) NULL,
    [LeaseStatus] nvarchar(max) NULL,
    [PropertyType] nvarchar(max) NULL,
    [Size] nvarchar(max) NULL,
    [Bedrooms] int NOT NULL,
    [Confidential] bit NOT NULL,
    [Featured] bit NOT NULL,
    [ShortDescription] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Additional] nvarchar(max) NULL,
    [Lease] nvarchar(max) NULL,
    [Areas] nvarchar(max) NULL,
    [Location] nvarchar(max) NULL,
    [AgentInfo] nvarchar(max) NULL,
    [Planning] nvarchar(max) NULL,
    [Rent] decimal(18,2) NULL,
    [AskingPrice] decimal(18,2) NULL,
    [Premium] decimal(18,2) NULL,
    [Fees] decimal(18,2) NULL,
    [RentDisplay] nvarchar(max) NULL,
    [AskingPriceDisplay] nvarchar(max) NULL,
    [PremiumDisplay] nvarchar(max) NULL,
    [FeesDisplay] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [QuickName] nvarchar(max) NULL,
    [Addressee] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [AgentId] nvarchar(max) NULL,
    [Floors] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodProperties] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [HoodPropertyFloorplans] (
    [Id] int NOT NULL IDENTITY,
    [PropertyId] int NOT NULL,
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
    [Directory] nvarchar(max) NULL,
    [GenericFileType] int NOT NULL,
    CONSTRAINT [PK_HoodPropertyFloorplans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodPropertyFloorplans_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodPropertyMedia] (
    [Id] int NOT NULL IDENTITY,
    [PropertyId] int NOT NULL,
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
    [Directory] nvarchar(max) NULL,
    [GenericFileType] int NOT NULL,
    CONSTRAINT [PK_HoodPropertyMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodPropertyMedia_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodPropertyMetadata] (
    [Id] int NOT NULL IDENTITY,
    [PropertyId] int NOT NULL,
    [BaseValue] nvarchar(max) NULL,
    [Name] nvarchar(450) NOT NULL,
    [Type] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodPropertyMetadata] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_HoodPropertyMetadata_PropertyId_Name] UNIQUE ([PropertyId], [Name]),
    CONSTRAINT [FK_HoodPropertyMetadata_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_HoodPropertyFloorplans_PropertyId] ON [HoodPropertyFloorplans] ([PropertyId]);
GO

CREATE INDEX [IX_HoodPropertyMedia_PropertyId] ON [HoodPropertyMedia] ([PropertyId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145759_v6.1', N'6.0.7');
GO

COMMIT;
GO

