using Hood.Extensions;

namespace Hood.Interfaces
{
    public interface IName
    {
        bool Anonymous { get; set; }
        string FullName { get; set; }
        string DisplayName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public static class INameExtensions
    {
        public static string ToDisplayName(this IName name, bool allowAnonymous = true)
        {
            if (name.Anonymous && allowAnonymous)
                return "Anonymous";
            if (name.DisplayName.IsSet())
                return name.DisplayName;
            return name.ToFullName();
        }
        public static string ToFullName(this IName name)
        {
            if (name.FullName.IsSet())
                return name.FullName;
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
