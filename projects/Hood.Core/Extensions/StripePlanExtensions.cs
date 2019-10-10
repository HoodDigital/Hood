namespace Hood.Extensions
{
    public static class StripePlanExtensions
    {
        public static string Price(this Stripe.Plan plan)
        {
            return ((double)plan.Amount / 100).ToString("C");
        }

        public static string FullPrice(this Stripe.Plan plan)
        {
            return ((double)plan.Amount / 100).ToString("C") + " every " + plan.IntervalCount + " " + plan.Interval + "(s)";
        }

    }
}