using Hood.Models;
using Microsoft.AspNetCore.Http;

namespace Hood.Extensions
{
    public static class HttpContextExtensions
    {
        public static AccountInfo GetAccountInfo(this HttpContext context)
        {
            return context.Items["AccountInfo"] as AccountInfo;
        }

        public static string GetSiteUrl(this HttpContext context, bool includePath = false, bool includeQuery = false)
        {
            // Return variable declaration
            var appPath = string.Empty;

            // Checking the current context content
            if (context?.Request != null)
            {
                //Formatting the fully qualified website url/name
                appPath = string.Format("{0}://{1}{2}",
                                        context.Request.Scheme,
                                        context.Request.Host,
                                        context.Request.PathBase);
                if (!appPath.EndsWith("/"))
                    appPath += "/";

                if (includePath)
                {
                    if (appPath.EndsWith("/"))
                        appPath = appPath.TrimEnd('/');
                    appPath += context.Request.Path;
                    if (includeQuery)
                        appPath += context.Request.QueryString.ToUriComponent();
                }

            }
            return appPath;
        }
    }
}
