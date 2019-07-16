using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Infrastructure
{
    public class CmsUrlConstraint : IRouteConstraint
    {
        private readonly object codeLock = new Object();

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {

            lock (codeLock)
            {
                string fullUrl = "";

                foreach (KeyValuePair<string, object> val in values.OrderBy(k => k.Key))
                {
                    if (val.Key != "area" && 
                        val.Key != "controller" &&
                        val.Key != "action" && 
                        val.Key != "id")
                    {
                        fullUrl += val.Value + "/";
                    }
                }
                if (fullUrl.EndsWith("/"))
                    fullUrl = fullUrl.TrimEnd('/');
                IContentRepository _content = (IContentRepository)httpContext.RequestServices.GetService(typeof(IContentRepository));
                IMemoryCache _cache = (IMemoryCache)httpContext.RequestServices.GetService(typeof(IMemoryCache));
                try
                {
                    string[] tokenised = fullUrl.ToLower().Split('/');
                    var type = Engine.Settings.Content.GetContentType(tokenised[0]);
                    if (type != null)
                    {
                        // if a type is matched, we must use the Hood routes, content CMS routes cannot be overridden.
                        if (tokenised.Length > 1)
                        {
                            switch (tokenised[1].ToLower())
                            {
                                case "search":
                                    values["action"] = "Search";
                                    if (!values.ContainsKey("type"))
                                        values.Add("type", tokenised[0]);
                                    return true;
                                case "category":
                                    values["action"] = "Category";
                                    if (!values.ContainsKey("id"))
                                        values.Add("id", int.Parse(tokenised[2]));
                                    if (!values.ContainsKey("type"))
                                        values.Add("type", tokenised[0]);
                                    return true;
                                case "author":
                                    values["action"] = "Author";
                                    if (!values.ContainsKey("id"))
                                        values.Add("id", tokenised[2]);
                                    if (!values.ContainsKey("type"))
                                        values.Add("type", tokenised[0]);
                                    return true;
                                case "property":
                                    return false;
                                default:
                                    int id;
                                    if (int.TryParse(tokenised[1], out id))
                                    {
                                        values["action"] = "Show";
                                        if (!values.ContainsKey("id"))
                                            values.Add("id", id);
                                        return true;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            values["action"] = "Feed";
                            if (!values.ContainsKey("type"))
                                values.Add("type", type.Type);
                            return true;
                        }
                    }
                    else
                    {
                        var settings = Engine.Settings.Property;
                        if (tokenised[0].ToLower() == settings.Slug)
                        {
                            values["action"] = "Index";
                            values["controller"] = "Property";
                            return true;
                        }
                    }
                    var pages = _content.GetPages();
                    if (pages.Select(p => p.Url).Contains(fullUrl))
                    {
                        var pg = pages.Where(p => p.Url == fullUrl).FirstOrDefault();
                        if (!values.ContainsKey("id"))
                            values.Add("id", pg.Id);
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
}