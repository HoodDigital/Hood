CREATE TABLE [HoodOptions] (
    [Id] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_HoodOptions] PRIMARY KEY ([Id])
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
    [GenericFileType] int NOT NULL,
    CONSTRAINT [PK_HoodMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodMedia_HoodMediaDirectories_DirectoryId] FOREIGN KEY ([DirectoryId]) REFERENCES [HoodMediaDirectories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_HoodMedia_DirectoryId] ON [HoodMedia] ([DirectoryId]);
GO

CREATE INDEX [IX_HoodMediaDirectories_ParentId] ON [HoodMediaDirectories] ([ParentId]);
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
    [AvatarId] int NULL,
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
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_HoodMedia_AvatarId] FOREIGN KEY ([AvatarId]) REFERENCES [HoodMedia] ([Id]) ON DELETE SET NULL
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
    [LastEditedOn] datetime2 NOT NULL,
    [Views] int NOT NULL,
    [ShareCount] int NOT NULL,
    [AllowComments] bit NOT NULL,
    [Public] bit NOT NULL,
    [Featured] bit NOT NULL,
    [AuthorId] nvarchar(450) NULL,
    [FeaturedImageId] int NULL,
    [ShareImageId] int NULL,
    CONSTRAINT [PK_HoodContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoodContent_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_HoodContent_HoodMedia_FeaturedImageId] FOREIGN KEY ([FeaturedImageId]) REFERENCES [HoodMedia] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_HoodContent_HoodMedia_ShareImageId] FOREIGN KEY ([ShareImageId]) REFERENCES [HoodMedia] ([Id]) ON DELETE SET NULL
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
	AspNetUsers.AvatarId, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
	AspNetUsers.UserVars,
	COUNT(AspNetRoles.Name) AS RoleCount,
	(
		SELECT 
			*
		FROM 
			AspNetAuth0Identities
		WHERE 
			AspNetAuth0Identities.LocalUserId = AspNetUsers.Id
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
	AspNetUsers.AvatarId, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
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
	HoodContent.FeaturedImageId,
	HoodContent.LastEditedBy,
	HoodContent.LastEditedOn,
	HoodContent.ParentId,
	HoodContent.[Public],
	HoodContent.PublishDate,
	HoodContent.ShareCount,
	HoodContent.Slug,
	HoodContent.[Status],
	HoodContent.Title,
	HoodContent.[Views],
	HoodContent.Featured,
	HoodContent.ShareImageId,
	AspNetUsers.FirstName,
	AspNetUsers.LastName,
	AspNetUsers.AvatarId,
	AspNetUsers.Anonymous,
	AspNetUsers.DisplayName,
	AspNetUsers.Email as AuthorEmail, 
	AspNetUsers.UserVars as AuthorVars
FROM
	HoodContent JOIN
	AspNetUsers ON HoodContent.AuthorId = AspNetUsers.Id
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
	AspNetUsers.AvatarId, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
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
	AspNetUsers.AvatarId, 
	AspNetUsers.LastLoginIP, 
	AspNetUsers.LastLoginLocation, 
	AspNetUsers.LastLogOn, 
	AspNetUsers.LockoutEnabled, 
	AspNetUsers.TwoFactorEnabled, 
	AspNetUsers.BillingAddressJson,	
	AspNetUsers.DeliveryAddressJson,	
	AspNetUsers.CreatedOn,	
	AspNetUsers.UserVars,
	AspNetUsers.AccessFailedCount
GO
