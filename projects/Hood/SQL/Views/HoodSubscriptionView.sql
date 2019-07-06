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
