using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Core
{
    public class PropertyRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            try
            {
                if (!Engine.Services.Installed) {
                    return false;
                }
                string type = null;
                if (values.ContainsKey("slug"))
                {
                    type = values["slug"].ToString();

                }
                if (type != null && type == Engine.Settings.Property.Slug)
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