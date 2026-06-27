using System.Collections.Generic;

namespace Hood.Services
{
    /// <summary>
    /// Single source of truth for content templates. Lists the selectable templates for a
    /// content type and reads a template's Razor source for meta-field parsing — covering
    /// compiled package views (RCL), the consumer application and the active theme, with no
    /// physical files required for packaged templates.
    /// </summary>
    public interface ITemplateProvider
    {
        /// <summary>
        /// All selectable templates within the given template folder (e.g. "Templates"),
        /// keyed by template name with a display-friendly value. Sources: app/theme physical
        /// views, then compiled views from the application and Hood's UI packages.
        /// </summary>
        Dictionary<string, string> GetTemplates(string templateFolder);

        /// <summary>
        /// Reads the Razor source of a template for meta-field parsing, honouring view
        /// precedence: app physical → active theme physical → packaged (embedded) sources.
        /// Returns null when the template cannot be found.
        /// </summary>
        string ReadTemplateSource(string templateFolder, string templateName);
    }
}
