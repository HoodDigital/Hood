namespace Hood.Enums
{
    public enum BillingMessage
    {
        NoStripeId,
        NoCustomerObject,
        NoSubscription,
        UpgradeRequired,
        NoPaymentSource,
        Error,
        InvalidToken,
        SubscriptionCreated,
        SubscriptionUpdated,
        SubscriptionEnded,
        SubscriptionCancelled,
        SubscriptionReactivated,
        SubscriptionExists
    }
}
