-- =============================================================================
-- Hood CMS — v6.1.x  ->  v7.0 upgrade delta
-- =============================================================================
-- Idempotent. Run once against a 6.1.x database (consumers below 6.1 should run
-- the 6.0/6.1 update scripts first — see sql/README.md for the chain).
--
-- The v7 changes are small: the legacy duplicate user tables are removed (AspNetUsers
-- is now the single authoritative user store), the unused Auth0 role-mapping column and
-- the legacy script-migration table are dropped, and the four reporting views are
-- rebuilt (the 6.1 definitions referenced columns that never existed on the base
-- tables and were silently broken). No data-bearing base-table columns are dropped.
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Detach HoodLogs from the legacy ApplicationUser table; HoodLogs.UserId stays as a plain column.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoodLogs_ApplicationUser_UserId')
    ALTER TABLE [HoodLogs] DROP CONSTRAINT [FK_HoodLogs_ApplicationUser_UserId];
GO

-- 2. Drop the legacy duplicate user tables. AspNetUsers (Identity) is authoritative; nothing
--    read or wrote these in 6.x. Dropping ApplicationUser also removes its FK to UserProfiles.
IF OBJECT_ID('[ApplicationUser]', 'U') IS NOT NULL DROP TABLE [ApplicationUser];
IF OBJECT_ID('[UserProfiles]', 'U')   IS NOT NULL DROP TABLE [UserProfiles];
GO

-- 3. Drop AspNetRoles.RemoteId — removed from the v7 role model (standard IdentityRole).
IF COL_LENGTH('AspNetRoles', 'RemoteId') IS NOT NULL ALTER TABLE [AspNetRoles] DROP COLUMN [RemoteId];
GO

-- 4. Drop the legacy Hood script-migration table — unused in v7 (version tracked in HoodOptions).
IF OBJECT_ID('[__HoodMigrationHistory]', 'U') IS NOT NULL DROP TABLE [__HoodMigrationHistory];
GO

-- 5. Rebuild the reporting views (idempotent DROP/CREATE).

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
	HoodContent.ParentId,
	HoodContent.[Public],
	HoodContent.PublishDate,
	HoodContent.ShareCount,
	HoodContent.Slug,
	HoodContent.[Status],
	HoodContent.Title,
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
	AspNetUsers.UserVars,
	AspNetUsers.AccessFailedCount
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
	HoodProperties LEFT OUTER JOIN
	AspNetUsers ON HoodProperties.AgentId = AspNetUsers.Id
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
	AspNetUsers.AvatarJson, 
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


-- 6. Stamp the schema version.
IF EXISTS (SELECT 1 FROM [HoodOptions] WHERE [Id] = 'Hood.Version')
    UPDATE [HoodOptions] SET [Value] = '7.0.0' WHERE [Id] = 'Hood.Version';
ELSE
    INSERT INTO [HoodOptions] ([Id], [Value]) VALUES ('Hood.Version', '7.0.0');
GO
