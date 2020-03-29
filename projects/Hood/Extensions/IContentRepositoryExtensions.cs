using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hood.Extensions
{
    public static class IContentRepositoryExtensions
    {
        public static List<string> GetMetasForTemplate(this IContentRepository contentRepository, string templateName, string folder)
        {
            templateName = templateName.Replace("Meta:", "");
            var _env = Engine.Services.Resolve<IWebHostEnvironment>();
            // get the right template file (from theme or if it doesnt appear there from base)
            string templatePath = _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
                templatePath = _env.ContentRootPath + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
                templatePath = _env.ContentRootPath + "\\UI\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
            {
                templatePath = null;
            }
            string template;
            if (templatePath != null)
            {
                // get the file contents 
                template = System.IO.File.ReadAllText(templatePath);
            }
            else
            {
                var path = "~/UI/" + folder + "/" + templateName + ".cshtml";
                if (UserInterfaceProvider.GetFiles(path).Length > 0)
                    template = UserInterfaceProvider.ReadAllText(path);
                else
                    return null;
            }

            // pull out any instance of @TemplateData["XXX"]
            Regex regex = new Regex(@"@ViewData\[\""(.*?)\""\]");
            List<string> metas = new List<string>();
            var matches = regex.Matches(template);
            foreach (Match mtch in matches)
            {
                var meta = mtch.Value.Replace("@ViewData[\"", "").Replace("\"]", "");
                if (meta.StartsWith("Template."))
                    metas.Add(meta);
            }

            regex = new Regex(@"@Html.Raw\(ViewData\[\""(.*?)\""\]\)");
            matches = regex.Matches(template);
            foreach (Match mtch in matches)
            {
                var meta = mtch.Value.Replace("@Html.Raw(ViewData[\"", "").Replace("\"])", "");
                if (meta.StartsWith("Template."))
                    metas.Add(meta);
            }
            // return list of all XXX metas.
            return metas.Distinct().ToList();
        }
    }
}
