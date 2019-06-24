using Hood.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Interfaces
{
    public interface IName
    {
        string FullName { get; set; }
        string DisplayName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public static class INameExtensions
    {
        public static string ToDisplayName(this IName name)
        {
            if (name.FullName.IsSet())
                return name.FullName;
            if (name.DisplayName.IsSet())
                return name.DisplayName;
            if (name.FirstName.IsSet() && name.LastName.IsSet())
                return name.FirstName + " " + name.LastName;
            else if (name.FirstName.IsSet() && !name.LastName.IsSet())
                return name.FirstName;
            else if (!name.FirstName.IsSet() && name.LastName.IsSet())
                return name.LastName;
            else return "";
        }
        public static string ToFullName(this IName name)
        {
            if (name.FirstName.IsSet() && name.LastName.IsSet())
                return name.FirstName + " " + name.LastName;
            else if (name.FirstName.IsSet() && !name.LastName.IsSet())
                return name.FirstName;
            else if (!name.FirstName.IsSet() && name.LastName.IsSet())
                return name.LastName;
            else return "";
        }
    }
}
