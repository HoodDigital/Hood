using System;

namespace Hood.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, T defaultValue)
        {
            var output = (T)Enum.Parse(typeof(T), value, true);
            return output != null ? output : defaultValue;
        }
    }
}
