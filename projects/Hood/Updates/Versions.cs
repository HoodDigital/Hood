using Hood.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hood
{
    public static class Versions
    {
        public static string Current()
        {
            return Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
        }

        /// <summary>
        /// This extension function is to update the HoodDbContext's to the latest version.
        /// </summary>
        /// <param name="previousVersion"></param>
        public static void ToLatest(this HoodDbContext context, Version previousVersion)
        {
            if (previousVersion.Major == 1)
            {
                if (previousVersion.Minor < 7)
                {
                    context.Update1_7();
                }
                if (previousVersion.Minor < 8)
                {
                    context.Update1_8();
                }
            }
            Option option = context.Options.Where(o => o.Id == "Hood.Version").FirstOrDefault();
            option.Value = JsonConvert.SerializeObject(Versions.Current());
            context.SaveChanges();
        }
    }
}
