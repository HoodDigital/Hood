using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Core
{
    public class PagesRouteConstraint : IRouteConstraint
    {
        private readonly object codeLock = new Object();

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            try
            {
                if (!Engine.Services.Installed) {
                    return false;
                }
                string fullUrl = httpContext.Request.Path;
                if (fullUrl.IsSet())
                {
                    fullUrl = fullUrl.ToString().ToLower().Trim('/');
                    IContentRepository _content = (IContentRepository)httpContext.RequestServices.GetService(typeof(IContentRepository));
                    var pages = _content.GetPages().Result;
                    var pg = pages.Where(p => p.Url.ToLower().Trim('/') == fullUrl).FirstOrDefault();
                    if (pg != null)
                    {
                        if (!values.ContainsKey("id"))
                            values.Add("id", pg.Id);
                        return true;
                    }
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