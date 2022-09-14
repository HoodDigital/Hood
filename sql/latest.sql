IF OBJECT_ID(N'[__HoodMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [__HoodMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        CONSTRAINT [PK___HoodMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

INSERT INTO [__HoodMigrationHistory] ([MigrationId])
VALUES (N'Hood_ScriptMigrations_v6.0');
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [RemoteId] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [Anonymous] bit NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [DisplayName] nvarchar(max) NULL,
    [BillingAddressJson] nvarchar(max) NULL,
    [DeliveryAddressJson] nvarchar(max) NULL,
    [AvatarJson] nvarchar(max) NULL,
    [UserVars] nvarchar(max) NULL,
    [Active] bit NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [LastLogOn] datetime2 NOT NULL,
    [LastLoginIP] nvarchar(max) NULL,
    [LastLoginLocation] nvarchar(max) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetAuth0Identities] (
    [Id] nvarchar(450) NOT NULL,
    [LocalUserId] nvarchar(450) NULL,
    [IsPrimary] bit NOT NULL,
    [Picture] nvarchar(max) NULL,
    [Provider] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetAuth0Identities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetAuth0Identities_AspNetUsers_LocalUserId] FOREIGN KEY ([LocalUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
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

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
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

COMMIT;
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

COMMIT;
GO

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

COMMIT;
GO

IF EXISTS(select * FROM sys.views where name = 'HoodAuth0UserProfiles') DROP VIEW HoodAuth0UserProfiles
GO
CREATE VIEW HoodAuth0UserProfiles AS
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

IF EXISTS(select * FROM sys.views where name = 'HoodContentViews') DROP VIEW HoodContentViews
GO
CREATE VIEW HoodContentViews AS
SELECT  
	HoodContent.Id,
	HoodContent.AllowComments,
	HoodContent.AuthorId,
	HoodContent.Body,
	HoodContent.ContentType,
	HoodContent.CreatedBy,
	HoodContent.CreatedOn,
	HoodContent.Excerpt,
	HoodContent.FeaturedImageJson,
	HoodContent.LastEditedBy,
	HoodContent.LastEditedOn,
	HoodContent.Notes,
	HoodContent.ParentId,
	HoodContent.[Public],
	HoodContent.PublishDate,
	HoodContent.ShareCount,
	HoodContent.Slug,
	HoodContent.[Status],
	HoodContent.SystemNotes,
	HoodContent.Title,
	HoodContent.UserVars,
	HoodContent.Views,
	HoodContent.Featured,
	HoodContent.ShareImageJson,
	AspNetUsers.FirstName,
	AspNetUsers.LastName,
	AspNetUsers.AvatarJson,
	AspNetUsers.Anonymous,
	AspNetUsers.DisplayName,
	AspNetUsers.Email as AuthorEmail, 
	AspNetUsers.UserVars as AuthorVars
FROM
	HoodContent JOIN
	AspNetUsers ON HoodContent.AuthorId = AspNetUsers.Id
GO

IF EXISTS(select * FROM sys.views where name = 'HoodPropertyViews') DROP VIEW HoodPropertyViews
GO
CREATE VIEW HoodPropertyViews AS
SELECT  
	HoodProperties.Id,
	HoodProperties.Additional,
	HoodProperties.Address1,
	HoodProperties.Address2,
	HoodProperties.AgentId,
	HoodProperties.AgentInfo,
	HoodProperties.AllowComments,
	HoodProperties.Areas,
	HoodProperties.AskingPrice,
	HoodProperties.AskingPriceDisplay,
	HoodProperties.Bedrooms,
	HoodProperties.City,
	HoodProperties.Confidential,
	HoodProperties.ContactName,
	HoodProperties.Country,
	HoodProperties.County,
	HoodProperties.CreatedBy,
	HoodProperties.CreatedOn,
	HoodProperties.[Description],
	HoodProperties.Featured,
	HoodProperties.FeaturedImageJson,
	HoodProperties.Fees,
	HoodProperties.FeesDisplay,
	HoodProperties.Floors,
	HoodProperties.InfoDownloadJson,
	HoodProperties.LastEditedBy,
	HoodProperties.LastEditedOn,
	HoodProperties.Latitude,
	HoodProperties.Lease,
	HoodProperties.ListingType,
	HoodProperties.[Location],
	HoodProperties.Longitude,
	HoodProperties.Notes,
	HoodProperties.Planning,
	HoodProperties.Postcode,
	HoodProperties.Premium,
	HoodProperties.PremiumDisplay,
	HoodProperties.PropertyType,
	HoodProperties.[Public],
	HoodProperties.PublishDate,
	HoodProperties.Reference,
	HoodProperties.Rent,
	HoodProperties.RentDisplay,
	HoodProperties.ShareCount,
	HoodProperties.ShortDescription,
	HoodProperties.Size,
	HoodProperties.[Status],
	HoodProperties.SystemNotes,
	HoodProperties.Tags,
	HoodProperties.Title,
	HoodProperties.UserVars,
	HoodProperties.[Views],
	HoodProperties.Number,
	HoodProperties.LeaseStatus,
	HoodProperties.Addressee,
	HoodProperties.Email,
	HoodProperties.Phone,
	HoodProperties.QuickName,
	AspNetUsers.FirstName,
	AspNetUsers.LastName,
	AspNetUsers.AvatarJson,
	AspNetUsers.Anonymous,
	AspNetUsers.DisplayName,
	AspNetUsers.Email as AgentEmail, 
	AspNetUsers.UserVars as AuthorVars
FROM
	HoodProperties JOIN
	AspNetUsers ON HoodProperties.AgentId = AspNetUsers.Id
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
	AspNetUsers.AccessFailedCount, 
	COUNT(AspNetRoles.Name) AS RoleCount,
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
	AspNetUsers.UserVars,
	AspNetUsers.AccessFailedCount
GO

INSERT INTO [__HoodMigrationHistory] ([MigrationId])
VALUES (N'Hood_ScriptMigrations_v6.1');
GO