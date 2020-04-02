using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, T defaultValue)
        {
            var output = (T)Enum.Parse(typeof(T), value, true);
            return output != null ? output : defaultValue;
        }
        public static IEnumerable<T> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
