IF OBJECT_ID(N'[__HoodMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [__HoodMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        CONSTRAINT [PK___HoodMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

ALTER TABLE [dbo].[HoodContent] DROP CONSTRAINT [FK_HoodContent_AspNetUsers_AuthorId]
GO

ALTER TABLE [dbo].[HoodProperties] DROP CONSTRAINT [FK_HoodProperties_AspNetUsers_AgentId]
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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145537_v6.1', N'6.0.7');
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145146_v6.1', N'6.0.7');
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145459_v6.1', N'6.0.7');
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220914145759_v6.1', N'6.0.7');
GO