using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Extensions
{
    public static class UrlHelpers
    {
        private static IHttpContextAccessor HttpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
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
