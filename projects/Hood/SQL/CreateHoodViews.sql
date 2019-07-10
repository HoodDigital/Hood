/* Subscriptions View */

IF EXISTS(select * FROM sys.views where name = 'HoodSubscription') DROP VIEW HoodSubscription
GO
IF EXISTS(select * FROM sys.views where name = 'HoodSubscriptionsView') DROP VIEW HoodSubscriptionsView
GO

CREATE VIEW HoodSubscriptionsView AS
SELECT        
	dbo.HoodSubscriptions.Id, 
	dbo.HoodSubscriptions.Addon, 
	dbo.HoodSubscriptions.Amount, 
	dbo.HoodSubscriptions.Colour, 
	dbo.HoodSubscriptions.Created, 
	dbo.HoodSubscriptions.CreatedBy, 
	dbo.HoodSubscriptions.Currency,
	dbo.HoodSubscriptions.Description, 
	dbo.HoodSubscriptions.FeaturedImageUrl, 
	dbo.HoodSubscriptions.Interval, 
	dbo.HoodSubscriptions.IntervalCount, 
	dbo.HoodSubscriptions.LastEditedBy, 
	dbo.HoodSubscriptions.LastEditedOn, 
	dbo.HoodSubscriptions.[Level], 
	dbo.HoodSubscriptions.LiveMode, 
	dbo.HoodSubscriptions.Name, 
	dbo.HoodSubscriptions.NumberAllowed, 
	dbo.HoodSubscriptions.[Public], 
	dbo.HoodSubscriptions.StatementDescriptor, 
	dbo.HoodSubscriptions.StripeId, 
	dbo.HoodSubscriptions.TrialPeriodDays, 
	dbo.HoodSubscriptions.ForumId, 
	dbo.HoodSubscriptions.TopicId, 
	COUNT(dbo.HoodUserSubscriptions.UserSubscriptionId) AS TotalCount,
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'active' THEN 1 ELSE NULL END) AS ActiveCount, 
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'trialing' THEN 1 ELSE NULL END) AS TrialCount, 
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'canceled' THEN 1 ELSE NULL END) AS InactiveCount
FROM            
	dbo.HoodUserSubscriptions INNER JOIN
	dbo.HoodSubscriptions ON dbo.HoodUserSubscriptions.SubscriptionId = dbo.HoodSubscriptions.Id
GROUP BY 
	dbo.HoodSubscriptions.Id, dbo.HoodSubscriptions.Amount, dbo.HoodSubscriptions.Colour, dbo.HoodSubscriptions.Created, dbo.HoodSubscriptions.CreatedBy, dbo.HoodSubscriptions.Currency, 
    dbo.HoodSubscriptions.Description, dbo.HoodSubscriptions.Interval, dbo.HoodSubscriptions.FeaturedImageUrl, dbo.HoodSubscriptions.IntervalCount, dbo.HoodSubscriptions.LastEditedBy, 
    dbo.HoodSubscriptions.LastEditedOn, dbo.HoodSubscriptions.[Level], dbo.HoodSubscriptions.Name, dbo.HoodSubscriptions.NumberAllowed, dbo.HoodSubscriptions.StatementDescriptor, dbo.HoodSubscriptions.StripeId, 
    dbo.HoodSubscriptions.TrialPeriodDays, dbo.HoodSubscriptions.ForumId, dbo.HoodSubscriptions.TopicId, dbo.HoodSubscriptions.Addon, dbo.HoodSubscriptions.LiveMode, dbo.HoodSubscriptions.[Public]
GO

/* UserSubscriptions - Used by HoodUserProfiles View */

IF EXISTS(select * FROM sys.views where name = 'HoodUserSubscriptionsView') DROP VIEW HoodUserSubscriptionsView
GO
CREATE VIEW HoodUserSubscriptionsView AS
SELECT        
	dbo.AspNetUsers.Id, 
	dbo.AspNetUsers.UserName, 
	dbo.AspNetUsers.Email, 
	dbo.AspNetUsers.EmailConfirmed, 
	dbo.AspNetUsers.PhoneNumber, 
	dbo.AspNetUsers.PhoneNumberConfirmed, 
	dbo.AspNetUsers.FirstName, 
	dbo.AspNetUsers.LastName, 
	dbo.AspNetUsers.DisplayName, 
	dbo.AspNetUsers.Active, 
	dbo.AspNetUsers.Anonymous, 
	dbo.AspNetUsers.AvatarJson, 
	dbo.AspNetUsers.LastLoginIP, 
	dbo.AspNetUsers.LastLoginLocation, 
	dbo.AspNetUsers.LastLogOn, 
	dbo.AspNetUsers.LockoutEnabled, 
	dbo.AspNetUsers.TwoFactorEnabled, 
	dbo.AspNetUsers.BillingAddressJson,	
	dbo.AspNetUsers.DeliveryAddressJson,	
	dbo.AspNetUsers.CreatedOn,	
	dbo.AspNetUsers.Latitude,	
	dbo.AspNetUsers.Longitude,	
	dbo.AspNetUsers.StripeId,	
	dbo.AspNetUsers.UserVars,
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'active' THEN 1 ELSE NULL END) AS ActiveCount, 
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'trialing' THEN 1 ELSE NULL END) AS TrialCount, 
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'canceled' THEN 1 ELSE NULL END) AS InactiveCount, 
	COUNT(CASE dbo.HoodUserSubscriptions.Status WHEN 'past_due' THEN 1 WHEN 'unpaid' THEN 1 ELSE NULL END) AS OverDueCount, 
	COUNT(dbo.HoodUserSubscriptions.UserSubscriptionId) AS TotalSubscriptions, 
	CONCAT
	(
		'[',
		STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					dbo.HoodUserSubscriptions.UserSubscriptionId IS NOT NULL
				THEN 
					CONCAT
					(
						'{', 
							'StripeSubscriptionId:', '"', dbo.HoodUserSubscriptions.StripeId, '",', 
							'StripeId:', '"', dbo.HoodSubscriptions.StripeId, '",', 
							'Status:', '"', dbo.HoodUserSubscriptions.Status, '",',
							'Name:', '"', dbo.HoodSubscriptions.Name, '",',
							'Category:', '"', dbo.HoodSubscriptions.Colour, '",',
							'Public:', '', dbo.HoodSubscriptions.[Public], ',',
							'Level:', '', dbo.HoodSubscriptions.Level, ',',
							'Addon:', '', dbo.HoodSubscriptions.Addon, ',',
							'CurrentPeriodEnd:', '"', dbo.HoodUserSubscriptions.CurrentPeriodEnd, '",',
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
	AS Subscriptions,
	STRING_AGG
		(
			ISNULL
			(
				CASE 
					dbo.HoodUserSubscriptions.Status
				WHEN 
					'active'
				THEN 
					dbo.HoodSubscriptions.StripeId
				WHEN 
					'trialing'
				THEN 
					dbo.HoodSubscriptions.StripeId
				ELSE 
					NULL 
				END,
				NULL
			)
			, ','
		)
	AS ActiveSubscriptionIds


FROM            
	dbo.HoodUserSubscriptions INNER JOIN
    dbo.HoodSubscriptions ON dbo.HoodUserSubscriptions.SubscriptionId = dbo.HoodSubscriptions.Id RIGHT OUTER JOIN
    dbo.AspNetUsers ON dbo.HoodUserSubscriptions.UserId = dbo.AspNetUsers.Id
GROUP BY 
	dbo.AspNetUsers.Id, 
	dbo.AspNetUsers.UserName, 
	dbo.AspNetUsers.Email, 
	dbo.AspNetUsers.EmailConfirmed, 
	dbo.AspNetUsers.PhoneNumber, 
	dbo.AspNetUsers.PhoneNumberConfirmed, 
	dbo.AspNetUsers.FirstName, 
	dbo.AspNetUsers.LastName, 
	dbo.AspNetUsers.DisplayName, 
	dbo.AspNetUsers.Active, 
	dbo.AspNetUsers.Anonymous, 
	dbo.AspNetUsers.AvatarJson, 
	dbo.AspNetUsers.LastLoginIP, 
	dbo.AspNetUsers.LastLoginLocation, 
	dbo.AspNetUsers.LastLogOn, 
	dbo.AspNetUsers.LockoutEnabled, 
	dbo.AspNetUsers.TwoFactorEnabled, 
	dbo.AspNetUsers.BillingAddressJson,	
	dbo.AspNetUsers.DeliveryAddressJson,	
	dbo.AspNetUsers.CreatedOn,	
	dbo.AspNetUsers.Latitude,	
	dbo.AspNetUsers.Longitude,	
	dbo.AspNetUsers.StripeId,	
	dbo.AspNetUsers.UserVars
GO

IF EXISTS(select * FROM sys.views where name = 'HoodUserProfiles') DROP VIEW HoodUserProfiles
GO
CREATE VIEW HoodUserProfiles AS
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
	dbo.HoodUserSubscriptionsView.ActiveSubscriptionIds,
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
	) AS Roles,
	STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					dbo.AspNetRoles.Id IS NOT NULL
				THEN 
					dbo.AspNetRoles.Id
				ELSE 
					NULL 
				END,
				NULL
			)
			, ','
		)
	AS RoleIds
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
	dbo.HoodUserSubscriptionsView.Subscriptions, 
	dbo.HoodUserSubscriptionsView.ActiveSubscriptionIds
GO