using System;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hood.Core
{
    public class ContentTypeRouteConstraint : IRouteConstraint
    {
        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection
        )
        {
            try
            {
                if (!Engine.Services.Installed)
                {
                    return false;
                }
                ContentType type = null;
                if (values.ContainsKey("type"))
                {
                    type = Engine.Settings.Content.GetContentType(values["type"].ToString());
                }
                if (type != null && !type.IsUnknown)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
