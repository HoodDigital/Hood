using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Services
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private IEnumerable<string> locs;

        public ViewLocationExpander()
        {
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            string theme = Engine.Settings != null ? Engine.Settings["Hood.Settings.Theme"] : null;
            var temp = new List<string>();

            // Add Themed Area views first.
            if (theme.IsSet())
                temp.AddRange(GetLocations("/Themes/" + theme + "/Areas/{2}/Views"));

            // Add Local Area views.
            temp.AddRange(GetLocations("/Areas/{2}/Views"));

            // Add Hood Packaged Area views.
            temp.AddRange(GetLocations("/Areas/{2}/UI"));

            // Now add Themed regular front end views.
            if (theme.IsSet())
                temp.AddRange(GetLocations("/Themes/" + theme + "/Views"));

            // Add Local Area front end views.
            temp.AddRange(GetLocations("/Views"));

            // Finally Hood Packaged  front end views.
            temp.AddRange(GetLocations("/UI"));

            locs = temp.AsEnumerable();
            context.Values["Hood.Settings.Theme"] = theme;
        }

        protected string[] GetLocations(string baseLocation)
        {
            return new[]
            {
                baseLocation + "/{0}.cshtml",
                baseLocation + "/{1}/{0}.cshtml",
                baseLocation + "/{1}/Partials/{0}.cshtml",
                baseLocation + "/Layouts/{1}/{0}.cshtml",
                baseLocation + "/Layouts/{0}.cshtml",
                baseLocation + "/Shared/{1}/{0}.cshtml",
                baseLocation + "/Shared/{0}.cshtml",
                baseLocation + "/Templates/{0}.cshtml",
                baseLocation + "/Components/{1}/{0}.cshtml",
                baseLocation + "/Components/{0}.cshtml"
            };
        }

        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return locs;
        }
    }
}
