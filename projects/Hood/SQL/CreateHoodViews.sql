/* Subscriptions View */

IF EXISTS(select * FROM sys.views where name = 'HoodSubscriptionPlans') DROP VIEW HoodSubscriptionPlans
GO

CREATE VIEW HoodSubscriptionPlans AS
SELECT        
	HoodSubscriptions.Id, 
	HoodSubscriptions.Addon, 
	HoodSubscriptions.Amount, 
	HoodSubscriptions.Colour, 
	HoodSubscriptions.Created, 
	HoodSubscriptions.CreatedBy, 
	HoodSubscriptions.Currency,
	HoodSubscriptions.Description, 
	HoodSubscriptions.Interval, 
	HoodSubscriptions.IntervalCount, 
	HoodSubscriptions.LastEditedBy, 
	HoodSubscriptions.LastEditedOn, 
	HoodSubscriptions.[Level], 
	HoodSubscriptions.LiveMode, 
	HoodSubscriptions.Name, 
	HoodSubscriptions.NumberAllowed, 
	HoodSubscriptions.[Public], 
	HoodSubscriptions.StatementDescriptor, 
	HoodSubscriptions.StripeId, 
	HoodSubscriptions.TrialPeriodDays, 
	HoodSubscriptions.SubscriptionProductId, 
	HoodSubscriptions.FeaturedImageJson, 
	HoodSubscriptions.FeaturesJson, 
	COUNT(HoodUserSubscriptions.UserSubscriptionId) AS TotalCount,
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'active' THEN 1 ELSE NULL END) AS ActiveCount, 
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'trialing' THEN 1 ELSE NULL END) AS TrialCount, 
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'canceled' THEN 1 ELSE NULL END) AS InactiveCount
FROM            
	HoodSubscriptions 
LEFT JOIN
	HoodUserSubscriptions ON HoodUserSubscriptions.SubscriptionId = HoodSubscriptions.Id
GROUP BY 
	HoodSubscriptions.Id, HoodSubscriptions.Amount, HoodSubscriptions.Colour, HoodSubscriptions.Created, HoodSubscriptions.CreatedBy, HoodSubscriptions.Currency, 
    HoodSubscriptions.Description, HoodSubscriptions.Interval, HoodSubscriptions.IntervalCount, HoodSubscriptions.LastEditedBy, 
    HoodSubscriptions.LastEditedOn, HoodSubscriptions.[Level], HoodSubscriptions.Name, HoodSubscriptions.NumberAllowed, HoodSubscriptions.StatementDescriptor, HoodSubscriptions.StripeId, 
    HoodSubscriptions.TrialPeriodDays, HoodSubscriptions.SubscriptionProductId, HoodSubscriptions.FeaturedImageJson, HoodSubscriptions.FeaturesJson, HoodSubscriptions.Addon, 
	HoodSubscriptions.LiveMode, HoodSubscriptions.[Public]
GO

/* UserSubscriptions - Used by HoodUserProfiles View */

IF EXISTS(select * FROM sys.views where name = 'HoodUserSubscriptionsView') DROP VIEW HoodUserSubscriptionsView
GO
CREATE VIEW HoodUserSubscriptionsView AS
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
	AspNetUsers.StripeId,	
	AspNetUsers.UserVars,
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'active' THEN 1 ELSE NULL END) AS ActiveCount, 
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'trialing' THEN 1 ELSE NULL END) AS TrialCount, 
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'canceled' THEN 1 ELSE NULL END) AS InactiveCount, 
	COUNT(CASE HoodUserSubscriptions.Status WHEN 'past_due' THEN 1 WHEN 'unpaid' THEN 1 ELSE NULL END) AS OverDueCount, 
	COUNT(HoodUserSubscriptions.UserSubscriptionId) AS TotalSubscriptions, 
	CONCAT
	(
		'[',
		STRING_AGG
		(
			ISNULL
			(
				CASE 
				WHEN 
					HoodUserSubscriptions.UserSubscriptionId IS NOT NULL
				THEN 
					CONCAT
					(
						'{', 
							'Id:', '', HoodUserSubscriptions.UserSubscriptionId, ',', 
							'PlanId:', '"', HoodSubscriptions.Id, '",', 
							'StripeSubscriptionId:', '"', HoodUserSubscriptions.StripeId, '",', 
							'StripeId:', '"', HoodSubscriptions.StripeId, '",', 
							'SubscriptionProductId:', '"', HoodSubscriptions.SubscriptionProductId, '",', 
							'Status:', '"', HoodUserSubscriptions.Status, '",',
							'Name:', '"', HoodSubscriptions.Name, '",',
							'Category:', '"', HoodSubscriptions.Colour, '",',
							'Public:', '', HoodSubscriptions.[Public], ',',
							'Level:', '', HoodSubscriptions.Level, ',',
							'Addon:', '', HoodSubscriptions.Addon, ',',
							'Amount:', '', HoodSubscriptions.Amount, ',',
							'Interval:', '"', HoodSubscriptions.Interval, '",',
							'IntervalCount:', '', HoodSubscriptions.IntervalCount, ',',
							'CurrentPeriodEnd:', '"', HoodUserSubscriptions.CurrentPeriodEnd, '",',
							'CancelAtPeriodEnd:', '', HoodUserSubscriptions.CancelAtPeriodEnd, ',',
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
					HoodUserSubscriptions.Status
				WHEN 
					'active'
				THEN 
					HoodSubscriptions.Id
				WHEN 
					'trialing'
				THEN 
					HoodSubscriptions.Id
				ELSE 
					NULL 
				END,
				NULL
			)
			, ','
		)
	AS ActiveSubscriptionIds


FROM            
	HoodUserSubscriptions INNER JOIN
    HoodSubscriptions ON HoodUserSubscriptions.SubscriptionId = HoodSubscriptions.Id RIGHT OUTER JOIN
    AspNetUsers ON HoodUserSubscriptions.UserId = AspNetUsers.Id
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
	AspNetUsers.StripeId,	
	AspNetUsers.UserVars
GO

IF EXISTS(select * FROM sys.views where name = 'HoodUserProfiles') DROP VIEW HoodUserProfiles
GO
CREATE VIEW HoodUserProfiles AS
SELECT 
	HoodUserSubscriptionsView.Id, 
	HoodUserSubscriptionsView.UserName, 
	HoodUserSubscriptionsView.Email, 
	HoodUserSubscriptionsView.EmailConfirmed, 
	HoodUserSubscriptionsView.PhoneNumber, 
	HoodUserSubscriptionsView.PhoneNumberConfirmed, 
	HoodUserSubscriptionsView.FirstName, 
	HoodUserSubscriptionsView.LastName, 
	HoodUserSubscriptionsView.DisplayName, 
	HoodUserSubscriptionsView.Active, 
	HoodUserSubscriptionsView.Anonymous, 
	HoodUserSubscriptionsView.AvatarJson, 
	HoodUserSubscriptionsView.LastLoginIP, 
	HoodUserSubscriptionsView.LastLoginLocation, 
	HoodUserSubscriptionsView.LastLogOn, 
	HoodUserSubscriptionsView.LockoutEnabled, 
	HoodUserSubscriptionsView.TwoFactorEnabled, 
	HoodUserSubscriptionsView.BillingAddressJson,
	HoodUserSubscriptionsView.DeliveryAddressJson,
	HoodUserSubscriptionsView.CreatedOn,
	HoodUserSubscriptionsView.Latitude,
	HoodUserSubscriptionsView.Longitude,
	HoodUserSubscriptionsView.StripeId,
	HoodUserSubscriptionsView.UserVars,
	HoodUserSubscriptionsView.ActiveCount, 
	HoodUserSubscriptionsView.TrialCount, 
	HoodUserSubscriptionsView.InactiveCount, 
	HoodUserSubscriptionsView.OverDueCount, 
	HoodUserSubscriptionsView.TotalSubscriptions, 
	HoodUserSubscriptionsView.Subscriptions,
	HoodUserSubscriptionsView.ActiveSubscriptionIds,
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
	HoodUserSubscriptionsView ON AspNetUserRoles.UserId = HoodUserSubscriptionsView.Id
GROUP BY
	HoodUserSubscriptionsView.Id, 
	HoodUserSubscriptionsView.UserName, 
	HoodUserSubscriptionsView.Email, 
	HoodUserSubscriptionsView.EmailConfirmed, 
	HoodUserSubscriptionsView.PhoneNumber, 
	HoodUserSubscriptionsView.PhoneNumberConfirmed, 
	HoodUserSubscriptionsView.FirstName, 
	HoodUserSubscriptionsView.LastName, 
	HoodUserSubscriptionsView.DisplayName, 
	HoodUserSubscriptionsView.Active, 
	HoodUserSubscriptionsView.Anonymous, 
	HoodUserSubscriptionsView.AvatarJson, 
	HoodUserSubscriptionsView.LastLoginIP, 
	HoodUserSubscriptionsView.LastLoginLocation, 
	HoodUserSubscriptionsView.LastLogOn, 
	HoodUserSubscriptionsView.LockoutEnabled, 
	HoodUserSubscriptionsView.TwoFactorEnabled, 
	HoodUserSubscriptionsView.BillingAddressJson,
	HoodUserSubscriptionsView.DeliveryAddressJson,
	HoodUserSubscriptionsView.CreatedOn,
	HoodUserSubscriptionsView.Latitude,
	HoodUserSubscriptionsView.Longitude,
	HoodUserSubscriptionsView.StripeId,
	HoodUserSubscriptionsView.UserVars,
	HoodUserSubscriptionsView.ActiveCount, 
	HoodUserSubscriptionsView.TrialCount, 
	HoodUserSubscriptionsView.InactiveCount, 
	HoodUserSubscriptionsView.OverDueCount, 
	HoodUserSubscriptionsView.TotalSubscriptions, 
	HoodUserSubscriptionsView.Subscriptions, 
	HoodUserSubscriptionsView.ActiveSubscriptionIds
GO