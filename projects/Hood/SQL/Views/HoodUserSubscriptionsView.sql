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
	AS Subscriptions

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
