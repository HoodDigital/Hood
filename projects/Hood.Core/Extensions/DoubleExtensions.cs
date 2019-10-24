using System;

namespace Hood.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRadian(this double degrees)
        {
            return (degrees * Math.PI / 180.0);
        }

        public static double ToDegrees(this double radians)
        {
            return (radians / Math.PI * 180.0);
        }

        public static string ToCurrencyString(this double amount)
        {
            return ((decimal)amount / 100).ToString("N2");
        }
        public static string ToCurrencyString(this decimal amount)
        {
            return ((decimal)amount / 100).ToString("N2");
        }
        public static string ToCurrencyString(this long amount)
        {
            return ((decimal)amount / 100).ToString("N2");
        }
        public static string ToCurrencyString(this int amount)
        {
            return ((decimal)amount / 100).ToString("N2");
        }

    }
}
