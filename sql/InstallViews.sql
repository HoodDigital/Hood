IF EXISTS(select * FROM sys.views where name = 'HoodSubscriptionPlans') DROP VIEW HoodSubscriptionPlans
GO

IF EXISTS(select * FROM sys.views where name = 'HoodUserSubscriptionsView') DROP VIEW HoodUserSubscriptionsView
GO