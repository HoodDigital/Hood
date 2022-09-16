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

CREATE TABLE [HoodContent] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Excerpt] nvarchar(max) NOT NULL,
    [Body] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [ParentId] int NULL,
    [PublishDate] datetime2 NOT NULL,
    [ContentType] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [AllowComments] bit NOT NULL,
    [Public] bit NOT NULL,
    [Featured] bit NOT NULL,
    [AuthorId] nvarchar(max) NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [ShareImageJson] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodContent] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [HoodContentCategories] (
    [ContentCategoryId] int NOT NULL IDENTITY,
    [DisplayName] nvarchar(max) NOT NULL,
    [Slug] nvarchar(max) NOT NULL,
    [ContentType] nvarchar(max) NULL,
    [ParentCategoryId] int NULL,
    CONSTRAINT [PK_HoodContentCategories] PRIMARY KEY ([ContentCategoryId]),
    CONSTRAINT [FK_HoodContentCategories_HoodContentCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [HoodContentCategories] ([ContentCategoryId])
);
GO

CREATE TABLE [HoodContentMedia] (
    [Id] int NOT NULL IDENTITY,
    [ContentId] int NOT NULL,
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
    CONSTRAINT [PK_HoodContentMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodContentMedia_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodContentMetadata] (
    [Id] int NOT NULL IDENTITY,
    [ContentId] int NOT NULL,
    [BaseValue] nvarchar(max) NULL,
    [Name] nvarchar(450) NOT NULL,
    [Type] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodContentMetadata] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_HoodContentMetadata_ContentId_Name] UNIQUE ([ContentId], [Name]),
    CONSTRAINT [FK_HoodContentMetadata_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodContentCategoryJoins] (
    [CategoryId] int NOT NULL,
    [ContentId] int NOT NULL,
    CONSTRAINT [PK_HoodContentCategoryJoins] PRIMARY KEY ([ContentId], [CategoryId]),
    CONSTRAINT [FK_HoodContentCategoryJoins_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_HoodContentCategoryJoins_HoodContentCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HoodContentCategories] ([ContentCategoryId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_HoodContentCategories_ParentCategoryId] ON [HoodContentCategories] ([ParentCategoryId]);
GO

CREATE INDEX [IX_HoodContentCategoryJoins_CategoryId] ON [HoodContentCategoryJoins] ([CategoryId]);
GO

CREATE INDEX [IX_HoodContentMedia_ContentId] ON [HoodContentMedia] ([ContentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145537_v6.1', N'6.0.7');
GO

COMMIT;
GO

