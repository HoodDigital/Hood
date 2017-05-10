using System.Collections.Generic;

namespace Hood.Models
{
    public static class Roles
    {
        private static readonly string[] roles = { "SuperUser", "Admin", "Editor", "Manager", "SEO", "ContactFormNotifications" };

        public static string SuperUser { get { return roles[0]; } }
        public static string Admin { get { return roles[1]; } }
        public static string Editor { get { return roles[2]; } }

        public static IEnumerable<string> All { get { return roles; } }
    }
}