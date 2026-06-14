-- Apply key 07.00.00/92 | Hood v7.0.0 - HoodUserProfiles reporting view (idempotent DROP/CREATE). Embedded; applied by hood-schema (DbUp) in LogicalName order.
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
