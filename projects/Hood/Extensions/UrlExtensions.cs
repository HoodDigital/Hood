using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;

namespace Hood.Extensions
{
    public static class UrlHelpers
    {
        private static IHttpContextAccessor HttpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public static string ToUrlString(this Uri absoluteUri, string hostname = null)
        {
            if (!hostname.IsSet())
            {
                try
                {
                    var mediaSettings = Engine.Current.Resolve<ISettingsRepository>().GetMediaSettings();
                    if (mediaSettings != null)
                    {
                        hostname = mediaSettings.AzureHost.IsSet() ? mediaSettings.AzureHost : null;
                    }
                } catch (Exception) { }
            }

            var host = hostname.IsSet() ? hostname : absoluteUri.Host;
            var scheme = Uri.UriSchemeHttps;
            var uriBuilder = new UriBuilder(absoluteUri)
            {
                Host = host,
                Scheme = scheme,
                Port = -1
            };
            return uriBuilder.Uri.AbsoluteUri;
        }


        public static Uri AddParameter(this Uri url, string name, string value)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[name] = value;
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Creates an absolute url based on the inputted action, controller etc.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string AbsoluteAction(this IUrlHelper url, string actionName, string controllerName, object routeValues = null)
        {
            string scheme = HttpContextAccessor.HttpContext.Request.Scheme;
            return url.Action(actionName, controllerName, routeValues, scheme);
        }

        /// <summary>
        /// Creates an absolute url based on the inputted url.
        /// </summary>
        /// <param name="url">The Url Helper class.</param>
        /// <param name="slug">The url slug, ommitting the first / - i.e. http://xxx.com/test/test would be test/test/</param>
        /// <returns></returns>
        public static string AbsoluteUrl(this IUrlHelper url, string slug = "")
        {
            var request = HttpContextAccessor.HttpContext.Request;
            return string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(), "/", slug); ;
        }
    }
}
