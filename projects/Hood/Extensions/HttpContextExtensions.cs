using Hood.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hood.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetSubdomain(this HttpContext httpContext)
        {

            string url = httpContext.Request.Headers["HOST"];
            var urlSegments = url.Split('.').ToList();
            switch (urlSegments.Count)
            {
                case 3:
                    // found subdomain - strip the first off, and then return it.
                    return urlSegments.First();
                default:
                    // FAIL - we should have exactly 3 substrings in order to return a sub-domain.
                    // Return "www" to signify normal site load.
                    return "www";
            }
        }

        public static void Set<T>(this HttpContext context, string key, T value)
        {
            context.Items[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>(this HttpContext context, string key)
        {
            var value = context.Items[key] as string;
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static AccountInfo GetAccountInfo(this HttpContext context)
        {
            return context.Items["AccountInfo"] as AccountInfo;
        }

        public static bool IsLockedOut(this HttpContext context, List<string> allowedCodes)
        {
            if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperUser"))
                return false;

            if (!context.Session.TryGetValue("LockoutModeToken", out byte[] betaCodeBytes))
                return true;
            var betaCode = System.Text.Encoding.Default.GetString(betaCodeBytes);

            if (allowedCodes.Contains(betaCode))
            {
                return false;
            }
            return true;
        }

        public static bool MatchesAccessCode(this HttpContext context, string code)
        {
            if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperUser"))
                return true;

            if (!context.Session.TryGetValue("LockoutModeToken", out byte[] betaCodeBytes))
                return false;
            var betaCode = System.Text.Encoding.Default.GetString(betaCodeBytes);

            if (betaCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
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
