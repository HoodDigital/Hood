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
	CONCAT
	(
		'[',
		STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					AspNetRoles.Id IS NOT NULL
				THEN 
					CONCAT
					(
						'{', 
							'Id:', '"', AspNetRoles.Id, '",', 
							'Name:', '"', AspNetRoles.Name, '",', 
							'NormalizedName:', '"', AspNetRoles.NormalizedName, '"', 
						'}'
					)
				ELSE 
					NULL 
				END,
				NULL
			)
			, ','
		),
		']'
	) AS Roles,
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

