IF OBJECT_ID(N'[__HoodMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [__HoodMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        CONSTRAINT [PK___HoodMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
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
    [Active] bit NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [LastLogOn] datetime2 NOT NULL,
    [LastLoginIP] nvarchar(max) NULL,
    [LastLoginLocation] nvarchar(max) NULL,
    [Latitude] nvarchar(max) NULL,
    [Longitude] nvarchar(max) NULL,
    [Anonymous] bit NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [DisplayName] nvarchar(max) NULL,
    [BillingAddressJson] nvarchar(max) NULL,
    [DeliveryAddressJson] nvarchar(max) NULL,
    [StripeId] nvarchar(max) NULL,
    [AvatarJson] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [HoodContentCategories] (
    [ContentCategoryId] int NOT NULL IDENTITY,
    [DisplayName] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [ContentType] nvarchar(max) NULL,
    [ParentCategoryId] int NULL,
    CONSTRAINT [PK_HoodContentCategories] PRIMARY KEY ([ContentCategoryId]),
    CONSTRAINT [FK_HoodContentCategories_HoodContentCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [HoodContentCategories] ([ContentCategoryId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodForumCategories] (
    [ForumCategoryId] int NOT NULL IDENTITY,
    [DisplayName] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [ParentCategoryId] int NULL,
    CONSTRAINT [PK_HoodForumCategories] PRIMARY KEY ([ForumCategoryId]),
    CONSTRAINT [FK_HoodForumCategories_HoodForumCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [HoodForumCategories] ([ForumCategoryId]) ON DELETE NO ACTION
);
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

CREATE TABLE [HoodSubscriptionGroups] (
    [Id] int NOT NULL IDENTITY,
    [DisplayName] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [Body] nvarchar(max) NULL,
    [Public] bit NOT NULL,
    [Created] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [StripeId] nvarchar(max) NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [FeaturesJson] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodSubscriptionGroups] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserAccessCodes] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(max) NULL,
    [Expiry] datetime2 NOT NULL,
    [Type] nvarchar(max) NULL,
    [Used] bit NOT NULL,
    [DateUsed] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_AspNetUserAccessCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserAccessCodes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodAddresses] (
    [Id] int NOT NULL IDENTITY,
    [ContactName] nvarchar(max) NULL,
    [QuickName] nvarchar(max) NULL,
    [Number] nvarchar(max) NOT NULL,
    [Address1] nvarchar(max) NULL,
    [Address2] nvarchar(max) NULL,
    [City] nvarchar(max) NOT NULL,
    [County] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    [Postcode] nvarchar(max) NOT NULL,
    [Latitude] float NOT NULL DEFAULT (0.0),
    [Longitude] float NOT NULL DEFAULT (0.0),
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_HoodAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodAddresses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodApiKeys] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NULL,
    [Key] nvarchar(max) NULL,
    [Active] bit NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [AccessLevel] int NOT NULL,
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_HoodApiKeys] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodApiKeys_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
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
    [UserVars] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [SystemNotes] nvarchar(max) NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [AllowComments] bit NOT NULL,
    [Public] bit NOT NULL,
    [Featured] bit NOT NULL,
    [AuthorId] nvarchar(450) NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [ShareImageJson] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodContent_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodForums] (
    [Id] int NOT NULL IDENTITY,
    [ViewingRequiresLogin] bit NOT NULL,
    [ViewingSubscriptions] nvarchar(max) NULL,
    [ViewingRoles] nvarchar(max) NULL,
    [PostingRequiresLogin] bit NOT NULL,
    [PostingSubscriptions] nvarchar(max) NULL,
    [PostingRoles] nvarchar(max) NULL,
    [AuthorId] nvarchar(450) NULL,
    [AuthorName] nvarchar(max) NULL,
    [AuthorDisplayName] nvarchar(max) NULL,
    [AuthorRoles] nvarchar(max) NULL,
    [CreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [LastPosted] datetime2 NULL,
    [LastTopicId] int NULL,
    [LastPostId] bigint NULL,
    [LastUserId] nvarchar(max) NULL,
    [LastUserName] nvarchar(max) NULL,
    [LastUserDisplayName] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [SystemNotes] nvarchar(max) NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [Published] bit NOT NULL,
    [Featured] bit NOT NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [ShareImageJson] nvarchar(max) NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Body] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [NumTopics] int NOT NULL,
    [NumPosts] int NOT NULL,
    [ModeratedPostCount] int NOT NULL,
    [RequireTopicModeration] bit NOT NULL,
    [RequirePostModeration] bit NOT NULL,
    CONSTRAINT [PK_HoodForums] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodForums_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
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
    CONSTRAINT [FK_HoodLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
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
    [AgentId] nvarchar(450) NULL,
    [Floors] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodProperties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodProperties_AspNetUsers_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodMedia] (
    [Id] int NOT NULL IDENTITY,
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
    [DirectoryId] int NULL,
    [Directory] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodMedia_HoodMediaDirectories_DirectoryId] FOREIGN KEY ([DirectoryId]) REFERENCES [HoodMediaDirectories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [StripeId] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Colour] nvarchar(max) NULL,
    [Public] bit NOT NULL,
    [Level] int NOT NULL,
    [Addon] bit NOT NULL,
    [NumberAllowed] int NOT NULL,
    [Amount] int NOT NULL,
    [Created] datetime2 NOT NULL,
    [Currency] nvarchar(max) NULL,
    [Interval] nvarchar(max) NULL,
    [IntervalCount] int NOT NULL,
    [LiveMode] bit NOT NULL,
    [StatementDescriptor] nvarchar(max) NULL,
    [TrialPeriodDays] int NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [SubscriptionProductId] int NULL,
    [FeaturesJson] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodSubscriptions_HoodSubscriptionGroups_SubscriptionProductId] FOREIGN KEY ([SubscriptionProductId]) REFERENCES [HoodSubscriptionGroups] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodApiEvents] (
    [Id] bigint NOT NULL IDENTITY,
    [ApiKeyId] nvarchar(450) NULL,
    [Time] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [Url] nvarchar(max) NULL,
    [Action] nvarchar(max) NULL,
    [RouteDataJson] nvarchar(max) NULL,
    [RequiredAccessLevel] int NOT NULL,
    CONSTRAINT [PK_HoodApiEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodApiEvents_HoodApiKeys_ApiKeyId] FOREIGN KEY ([ApiKeyId]) REFERENCES [HoodApiKeys] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodContentCategoryJoins] (
    [CategoryId] int NOT NULL,
    [ContentId] int NOT NULL,
    CONSTRAINT [PK_HoodContentCategoryJoins] PRIMARY KEY ([ContentId], [CategoryId]),
    CONSTRAINT [FK_HoodContentCategoryJoins_HoodContentCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HoodContentCategories] ([ContentCategoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_HoodContentCategoryJoins_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodContentMedia] (
    [Id] int NOT NULL IDENTITY,
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
    [ContentId] int NOT NULL,
    CONSTRAINT [PK_HoodContentMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodContentMedia_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodContentMetadata] (
    [Id] int NOT NULL IDENTITY,
    [BaseValue] nvarchar(max) NULL,
    [Name] nvarchar(450) NOT NULL,
    [Type] nvarchar(max) NULL,
    [ContentId] int NOT NULL,
    CONSTRAINT [PK_HoodContentMetadata] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_HoodContentMetadata_ContentId_Name] UNIQUE ([ContentId], [Name]),
    CONSTRAINT [FK_HoodContentMetadata_HoodContent_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [HoodContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodForumCategoryJoins] (
    [CategoryId] int NOT NULL,
    [ForumId] int NOT NULL,
    CONSTRAINT [PK_HoodForumCategoryJoins] PRIMARY KEY ([ForumId], [CategoryId]),
    CONSTRAINT [FK_HoodForumCategoryJoins_HoodForumCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HoodForumCategories] ([ForumCategoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_HoodForumCategoryJoins_HoodForums_ForumId] FOREIGN KEY ([ForumId]) REFERENCES [HoodForums] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodForumTopics] (
    [Id] int NOT NULL IDENTITY,
    [ViewingRequiresLogin] bit NOT NULL,
    [ViewingSubscriptions] nvarchar(max) NULL,
    [ViewingRoles] nvarchar(max) NULL,
    [PostingRequiresLogin] bit NOT NULL,
    [PostingSubscriptions] nvarchar(max) NULL,
    [PostingRoles] nvarchar(max) NULL,
    [AuthorId] nvarchar(450) NULL,
    [AuthorName] nvarchar(max) NULL,
    [AuthorDisplayName] nvarchar(max) NULL,
    [AuthorRoles] nvarchar(max) NULL,
    [CreatedOn] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastEditedOn] datetime2 NOT NULL,
    [LastEditedBy] nvarchar(max) NULL,
    [LastPosted] datetime2 NULL,
    [LastTopicId] int NULL,
    [LastPostId] bigint NULL,
    [LastUserId] nvarchar(max) NULL,
    [LastUserName] nvarchar(max) NULL,
    [LastUserDisplayName] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [SystemNotes] nvarchar(max) NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [Published] bit NOT NULL,
    [Featured] bit NOT NULL,
    [FeaturedImageJson] nvarchar(max) NULL,
    [ShareImageJson] nvarchar(max) NULL,
    [ForumId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Approved] bit NOT NULL,
    [ApprovedTime] datetime2 NULL,
    [NumPosts] int NOT NULL,
    [ModeratedPostCount] int NOT NULL,
    [AllowReplies] bit NOT NULL,
    CONSTRAINT [PK_HoodForumTopics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodForumTopics_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoodForumTopics_HoodForums_ForumId] FOREIGN KEY ([ForumId]) REFERENCES [HoodForums] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodPropertyFloorplans] (
    [Id] int NOT NULL IDENTITY,
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
    [PropertyId] int NOT NULL,
    CONSTRAINT [PK_HoodPropertyFloorplans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodPropertyFloorplans_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodPropertyMedia] (
    [Id] int NOT NULL IDENTITY,
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
    [PropertyId] int NOT NULL,
    CONSTRAINT [PK_HoodPropertyMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodPropertyMedia_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodPropertyMetadata] (
    [Id] int NOT NULL IDENTITY,
    [BaseValue] nvarchar(max) NULL,
    [Name] nvarchar(450) NOT NULL,
    [Type] nvarchar(max) NULL,
    [PropertyId] int NOT NULL,
    CONSTRAINT [PK_HoodPropertyMetadata] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_HoodPropertyMetadata_PropertyId_Name] UNIQUE ([PropertyId], [Name]),
    CONSTRAINT [FK_HoodPropertyMetadata_HoodProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HoodProperties] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodSubscriptionFeatures] (
    [Id] int NOT NULL IDENTITY,
    [BaseValue] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [Type] nvarchar(max) NULL,
    [SubscriptionId] int NOT NULL,
    CONSTRAINT [PK_HoodSubscriptionFeatures] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodSubscriptionFeatures_HoodSubscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [HoodSubscriptions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HoodUserSubscriptions] (
    [UserSubscriptionId] int NOT NULL IDENTITY,
    [Confirmed] bit NOT NULL,
    [Deleted] bit NOT NULL,
    [StripeId] nvarchar(max) NULL,
    [CancelAtPeriodEnd] bit NOT NULL,
    [CanceledAt] datetime2 NULL,
    [Created] datetime2 NULL,
    [CurrentPeriodEnd] datetime2 NULL,
    [CurrentPeriodStart] datetime2 NULL,
    [CustomerId] nvarchar(max) NULL,
    [EndedAt] datetime2 NULL,
    [Quantity] bigint NOT NULL,
    [Start] datetime2 NULL,
    [Status] nvarchar(max) NULL,
    [TaxPercent] decimal(18,2) NULL,
    [TrialEnd] datetime2 NULL,
    [TrialStart] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [LastUpdated] datetime2 NOT NULL,
    [DeletedAt] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    [SubscriptionId] int NOT NULL,
    CONSTRAINT [PK_HoodUserSubscriptions] PRIMARY KEY ([UserSubscriptionId]),
    CONSTRAINT [FK_HoodUserSubscriptions_HoodSubscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [HoodSubscriptions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_HoodUserSubscriptions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoodForumPosts] (
    [Id] bigint NOT NULL IDENTITY,
    [TopicId] int NOT NULL,
    [ReplyId] bigint NULL,
    [AuthorId] nvarchar(450) NULL,
    [AuthorName] nvarchar(max) NULL,
    [AuthorDisplayName] nvarchar(max) NULL,
    [AuthorIp] nvarchar(max) NULL,
    [AuthorRoles] nvarchar(max) NULL,
    [PostedTime] datetime2 NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [Signature] nvarchar(max) NULL,
    [Approved] bit NOT NULL,
    [ApprovedTime] datetime2 NULL,
    [Edited] bit NOT NULL,
    [EditReason] nvarchar(max) NULL,
    [EditedTime] datetime2 NULL,
    [EditedById] nvarchar(450) NULL,
    [Deleted] bit NOT NULL,
    [DeleteReason] nvarchar(max) NULL,
    [DeletedTime] datetime2 NULL,
    [DeletedById] nvarchar(450) NULL,
    CONSTRAINT [PK_HoodForumPosts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodForumPosts_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoodForumPosts_AspNetUsers_DeletedById] FOREIGN KEY ([DeletedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoodForumPosts_AspNetUsers_EditedById] FOREIGN KEY ([EditedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoodForumPosts_HoodForumPosts_ReplyId] FOREIGN KEY ([ReplyId]) REFERENCES [HoodForumPosts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoodForumPosts_HoodForumTopics_TopicId] FOREIGN KEY ([TopicId]) REFERENCES [HoodForumTopics] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserAccessCodes_UserId] ON [AspNetUserAccessCodes] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_HoodAddresses_UserId] ON [HoodAddresses] ([UserId]);
GO

CREATE INDEX [IX_HoodApiEvents_ApiKeyId] ON [HoodApiEvents] ([ApiKeyId]);
GO

CREATE INDEX [IX_HoodApiKeys_UserId] ON [HoodApiKeys] ([UserId]);
GO

CREATE INDEX [IX_HoodContent_AuthorId] ON [HoodContent] ([AuthorId]);
GO

CREATE INDEX [IX_HoodContentCategories_ParentCategoryId] ON [HoodContentCategories] ([ParentCategoryId]);
GO

CREATE INDEX [IX_HoodContentCategoryJoins_CategoryId] ON [HoodContentCategoryJoins] ([CategoryId]);
GO

CREATE INDEX [IX_HoodContentMedia_ContentId] ON [HoodContentMedia] ([ContentId]);
GO

CREATE INDEX [IX_HoodForumCategories_ParentCategoryId] ON [HoodForumCategories] ([ParentCategoryId]);
GO

CREATE INDEX [IX_HoodForumCategoryJoins_CategoryId] ON [HoodForumCategoryJoins] ([CategoryId]);
GO

CREATE INDEX [IX_HoodForumPosts_AuthorId] ON [HoodForumPosts] ([AuthorId]);
GO

CREATE INDEX [IX_HoodForumPosts_DeletedById] ON [HoodForumPosts] ([DeletedById]);
GO

CREATE INDEX [IX_HoodForumPosts_EditedById] ON [HoodForumPosts] ([EditedById]);
GO

CREATE INDEX [IX_HoodForumPosts_ReplyId] ON [HoodForumPosts] ([ReplyId]);
GO

CREATE INDEX [IX_HoodForumPosts_TopicId] ON [HoodForumPosts] ([TopicId]);
GO

CREATE INDEX [IX_HoodForums_AuthorId] ON [HoodForums] ([AuthorId]);
GO

CREATE INDEX [IX_HoodForumTopics_AuthorId] ON [HoodForumTopics] ([AuthorId]);
GO

CREATE INDEX [IX_HoodForumTopics_ForumId] ON [HoodForumTopics] ([ForumId]);
GO

CREATE INDEX [IX_HoodLogs_UserId] ON [HoodLogs] ([UserId]);
GO

CREATE INDEX [IX_HoodMedia_DirectoryId] ON [HoodMedia] ([DirectoryId]);
GO

CREATE INDEX [IX_HoodMediaDirectories_ParentId] ON [HoodMediaDirectories] ([ParentId]);
GO

CREATE INDEX [IX_HoodProperties_AgentId] ON [HoodProperties] ([AgentId]);
GO

CREATE INDEX [IX_HoodPropertyFloorplans_PropertyId] ON [HoodPropertyFloorplans] ([PropertyId]);
GO

CREATE INDEX [IX_HoodPropertyMedia_PropertyId] ON [HoodPropertyMedia] ([PropertyId]);
GO

CREATE INDEX [IX_HoodSubscriptionFeatures_SubscriptionId] ON [HoodSubscriptionFeatures] ([SubscriptionId]);
GO

CREATE INDEX [IX_HoodSubscriptions_SubscriptionProductId] ON [HoodSubscriptions] ([SubscriptionProductId]);
GO

CREATE INDEX [IX_HoodUserSubscriptions_SubscriptionId] ON [HoodUserSubscriptions] ([SubscriptionId]);
GO

CREATE INDEX [IX_HoodUserSubscriptions_UserId] ON [HoodUserSubscriptions] ([UserId]);
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [HoodApiEvents];
GO

DROP TABLE [HoodForumCategoryJoins];
GO

DROP TABLE [HoodForumPosts];
GO

DROP TABLE [HoodSubscriptionFeatures];
GO

DROP TABLE [HoodUserSubscriptions];
GO

DROP TABLE [HoodApiKeys];
GO

DROP TABLE [HoodForumCategories];
GO

DROP TABLE [HoodForumTopics];
GO

DROP TABLE [HoodSubscriptions];
GO

DROP TABLE [HoodForums];
GO

DROP TABLE [HoodSubscriptionGroups];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'StripeId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [StripeId];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HoodAddresses]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HoodAddresses] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HoodAddresses] ALTER COLUMN [Number] nvarchar(max) NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HoodAddresses]') AND [c].[name] = N'Address1');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [HoodAddresses] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [HoodAddresses] ALTER COLUMN [Address1] nvarchar(max) NOT NULL;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [AspNetUserAccessCodes];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HoodContentCategories]') AND [c].[name] = N'Slug');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HoodContentCategories] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [HoodContentCategories] ALTER COLUMN [Slug] nvarchar(max) NOT NULL;
ALTER TABLE [HoodContentCategories] ADD DEFAULT N'' FOR [Slug];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HoodContentCategories]') AND [c].[name] = N'DisplayName');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [HoodContentCategories] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [HoodContentCategories] ALTER COLUMN [DisplayName] nvarchar(max) NOT NULL;
ALTER TABLE [HoodContentCategories] ADD DEFAULT N'' FOR [DisplayName];
GO

ALTER TABLE [AspNetRoles] ADD [RemoteId] nvarchar(max) NULL;
GO

CREATE TABLE [AspNetAuth0Identities] (
    [Id] nvarchar(450) NOT NULL,
    [LocalUserId] nvarchar(450) NULL,
    [IsPrimary] bit NOT NULL,
    [Picture] nvarchar(max) NULL,
    [Connection] nvarchar(max) NULL,
    [IsSocial] bit NULL,
    [Provider] nvarchar(max) NULL,
    [UserId] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetAuth0Identities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetAuth0Identities_AspNetUsers_LocalUserId] FOREIGN KEY ([LocalUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetAuth0Identities_LocalUserId] ON [AspNetAuth0Identities] ([LocalUserId]);
GO

COMMIT;
GO

IF EXISTS(select * FROM sys.views where name = 'HoodUserProfiles') DROP VIEW HoodUserProfiles
GO
CREATE VIEW HoodUserProfiles AS
SELECT 
	AspNetUsers.Id, 
	AspNetUsers.UserName, 
	AspNetUsers.Email, 
	AspNetUsers.EmailConfirmed, 
	AspNetUsers.PhoneNumber, 
	AspNetUsers.PhoneNumberConfirmed, 
	AspNetUsers.FirstName, 
	AspNetUsers.LastName, 
	AspNetUsers.DisplayName, 
	AspNetUsers.Active, 
	AspNetUsers.Anonymous, 
	AspNetUsers.AvatarJson, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
	AspNetUsers.Latitude,	
	AspNetUsers.Longitude,	
	AspNetUsers.UserVars,
	COUNT(AspNetRoles.Name) AS RoleCount,
	(
		SELECT 
			*
		FROM 
			AspNetAuth0Identities
		WHERE 
			AspNetAuth0Identities.UserId = AspNetUsers.Id
		FOR JSON AUTO
	) AS Auth0UsersJson,
	(
		SELECT 
			AspNetRoles.Id, AspNetRoles.Name, AspNetRoles.NormalizedName 
		FROM 
			AspNetUserRoles INNER JOIN
			AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id
		WHERE 
			AspNetUserRoles.UserId = AspNetUsers.Id
		FOR JSON AUTO
	) AS RolesJson,
	STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					AspNetRoles.Id IS NOT NULL
				THEN 
					AspNetRoles.Id
				ELSE 
					NULL 
				END,
				NULL
			)
			, ','
		)
	AS RoleIds
FROM
	AspNetUserRoles INNER JOIN
	AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id RIGHT OUTER JOIN
	AspNetUsers ON AspNetUserRoles.UserId = AspNetUsers.Id
GROUP BY
	AspNetUsers.Id, 
	AspNetUsers.UserName, 
	AspNetUsers.Email, 
	AspNetUsers.EmailConfirmed, 
	AspNetUsers.PhoneNumber, 
	AspNetUsers.PhoneNumberConfirmed, 
	AspNetUsers.FirstName, 
	AspNetUsers.LastName, 
	AspNetUsers.DisplayName, 
	AspNetUsers.Active, 
	AspNetUsers.Anonymous, 
	AspNetUsers.AvatarJson, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
	AspNetUsers.Latitude,	
	AspNetUsers.Longitude,	
	AspNetUsers.UserVars
GO

/* Remove old obsolete views */

IF EXISTS(select * FROM sys.views where name = 'HoodSubscriptionPlans') DROP VIEW HoodSubscriptionPlans
GO

IF EXISTS(select * FROM sys.views where name = 'HoodUserSubscriptionsView') DROP VIEW HoodUserSubscriptionsView
GO

INSERT INTO [__HoodMigrationHistory] ([MigrationId])
VALUES (N'Hood_ScriptMigrations_v6.0');
GO