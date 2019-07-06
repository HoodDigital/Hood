SELECT 
dbo.HoodUserSubscriptionsView.Id, 
dbo.HoodUserSubscriptionsView.UserName, 
dbo.HoodUserSubscriptionsView.Email, 
dbo.HoodUserSubscriptionsView.EmailConfirmed, 
dbo.HoodUserSubscriptionsView.PhoneNumber, 
dbo.HoodUserSubscriptionsView.PhoneNumberConfirmed, 
dbo.HoodUserSubscriptionsView.FirstName, 
dbo.HoodUserSubscriptionsView.LastName, 
dbo.HoodUserSubscriptionsView.DisplayName, 
dbo.HoodUserSubscriptionsView.Active, 
dbo.HoodUserSubscriptionsView.Anonymous, 
dbo.HoodUserSubscriptionsView.AvatarJson, 
dbo.HoodUserSubscriptionsView.LastLoginIP, 
dbo.HoodUserSubscriptionsView.LastLoginLocation, 
dbo.HoodUserSubscriptionsView.LastLogOn, 
dbo.HoodUserSubscriptionsView.LockoutEnabled, 
dbo.HoodUserSubscriptionsView.TwoFactorEnabled, 
dbo.HoodUserSubscriptionsView.BillingAddressJson,
dbo.HoodUserSubscriptionsView.DeliveryAddressJson,
dbo.HoodUserSubscriptionsView.CreatedOn,
dbo.HoodUserSubscriptionsView.Latitude,
dbo.HoodUserSubscriptionsView.Longitude,
dbo.HoodUserSubscriptionsView.StripeId,
dbo.HoodUserSubscriptionsView.UserVars,
dbo.HoodUserSubscriptionsView.ActiveCount, 
dbo.HoodUserSubscriptionsView.TrialCount, 
dbo.HoodUserSubscriptionsView.InactiveCount, 
dbo.HoodUserSubscriptionsView.OverDueCount, 
dbo.HoodUserSubscriptionsView.TotalSubscriptions, 
dbo.HoodUserSubscriptionsView.Subscriptions,
COUNT(dbo.AspNetRoles.Name) AS RoleCount,
CONCAT
	(
		'[',
		STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					dbo.AspNetRoles.Id IS NOT NULL
				THEN 
					CONCAT
					(
						'{', 
							'Id:', '"', dbo.AspNetRoles.Id, '",', 
							'Name:', '"', dbo.AspNetRoles.Name, '",', 
							'NormalizedName:', '"', dbo.AspNetRoles.NormalizedName, '"', 
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
	)
	AS Roles
FROM            
dbo.AspNetUserRoles INNER JOIN
dbo.AspNetRoles ON dbo.AspNetUserRoles.RoleId = dbo.AspNetRoles.Id RIGHT OUTER JOIN
dbo.HoodUserSubscriptionsView ON dbo.AspNetUserRoles.UserId = dbo.HoodUserSubscriptionsView.Id
GROUP BY
dbo.HoodUserSubscriptionsView.Id, 
dbo.HoodUserSubscriptionsView.UserName, 
dbo.HoodUserSubscriptionsView.Email, 
dbo.HoodUserSubscriptionsView.EmailConfirmed, 
dbo.HoodUserSubscriptionsView.PhoneNumber, 
dbo.HoodUserSubscriptionsView.PhoneNumberConfirmed, 
dbo.HoodUserSubscriptionsView.FirstName, 
dbo.HoodUserSubscriptionsView.LastName, 
dbo.HoodUserSubscriptionsView.DisplayName, 
dbo.HoodUserSubscriptionsView.Active, 
dbo.HoodUserSubscriptionsView.Anonymous, 
dbo.HoodUserSubscriptionsView.AvatarJson, 
dbo.HoodUserSubscriptionsView.LastLoginIP, 
dbo.HoodUserSubscriptionsView.LastLoginLocation, 
dbo.HoodUserSubscriptionsView.LastLogOn, 
dbo.HoodUserSubscriptionsView.LockoutEnabled, 
dbo.HoodUserSubscriptionsView.TwoFactorEnabled, 
dbo.HoodUserSubscriptionsView.BillingAddressJson,
dbo.HoodUserSubscriptionsView.DeliveryAddressJson,
dbo.HoodUserSubscriptionsView.CreatedOn,
dbo.HoodUserSubscriptionsView.Latitude,
dbo.HoodUserSubscriptionsView.Longitude,
dbo.HoodUserSubscriptionsView.StripeId,
dbo.HoodUserSubscriptionsView.UserVars,
dbo.HoodUserSubscriptionsView.ActiveCount, 
dbo.HoodUserSubscriptionsView.TrialCount, 
dbo.HoodUserSubscriptionsView.InactiveCount, 
dbo.HoodUserSubscriptionsView.OverDueCount, 
dbo.HoodUserSubscriptionsView.TotalSubscriptions, 
dbo.HoodUserSubscriptionsView.Subscriptions