using Stripe;

namespace Hood.Models
{
    public static class StripePlanExtensions
    {
        public static string Price(this StripePlan plan)
        {
            return ((double)plan.Amount / 100).ToString("C");
        }

        public static string FullPrice(this StripePlan plan)
        {
            return ((double)plan.Amount / 100).ToString("C") + " every " + plan.IntervalCount + " " + plan.Interval + "(s)";
        }

    }
}