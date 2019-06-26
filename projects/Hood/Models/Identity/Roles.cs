using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public static class Roles
    {
        private static readonly string[] system = { "SuperUser", "Admin", "Editor", "Api", "Moderator", "ContactFormNotifications", "NewAccountNotifications" };
        private static readonly string[] roles = { };

        public static IEnumerable<string> System { get { return system; } }
        public static IEnumerable<string> All { get { return system.Concat(roles); } }
    }
}