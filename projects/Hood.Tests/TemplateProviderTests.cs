using System.Linq;
using Hood.Services;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// template discovery logic. The pure path/dedupe internals are tested directly;
    /// end-to-end enumeration (compiled views + physical providers) is covered by the docker
    /// e2e pass since it needs a composed MVC application.
    /// </summary>
    public class TemplateProviderTests
    {
        [Theory]
        [InlineData("/UI/Templates/_Full.cshtml", true)]
        [InlineData("/BaseUI/Templates/_Full.cshtml", true)]
        [InlineData("/Views/Templates/_Custom.cshtml", true)]
        [InlineData("/Themes/mytheme/Views/Templates/_Themed.cshtml", true)]
        [InlineData("/Themes/othertheme/Views/Templates/_Other.cshtml", false)] // inactive theme
        [InlineData("/UI/Home/Index.cshtml", false)] // not a template folder
        [InlineData("/UI/Templates/readme.txt", false)] // not razor
        [InlineData(null, false)]
        public void IsTemplatePath_filters_to_active_sources(string path, bool expected)
        {
            Assert.Equal(expected, TemplateProvider.IsTemplatePath(path, "Templates", "mytheme"));
        }

        [Fact]
        public void IsTemplatePath_ignores_theme_paths_when_no_theme_active()
        {
            Assert.False(
                TemplateProvider.IsTemplatePath(
                    "/Themes/mytheme/Views/Templates/_Themed.cshtml",
                    "Templates",
                    null
                )
            );
        }

        [Fact]
        public void AddTemplate_dedupes_first_wins_and_prettifies_names()
        {
            var templates = new System.Collections.Generic.Dictionary<string, string>();
            TemplateProvider.AddTemplate(templates, "_Full_with_Hero.cshtml");
            TemplateProvider.AddTemplate(templates, "_Full_with_Hero.cshtml"); // duplicate ignored
            TemplateProvider.AddTemplate(templates, "_Contact.cshtml");

            Assert.Equal(2, templates.Count);
            Assert.Equal("Full With Hero", templates["_Full_with_Hero"]);
            Assert.Equal("Contact", templates["_Contact"]);
        }

        [Fact]
        public void PhysicalTemplateFolders_orders_app_then_theme_then_legacy_ui()
        {
            var folders = TemplateProvider.PhysicalTemplateFolders("Templates", "demo").ToList();
            Assert.Equal(
                new[] { "Views/Templates", "Themes/demo/Views/Templates", "UI/Templates" },
                folders
            );
        }

        [Fact]
        public void PhysicalTemplateFolders_skips_theme_when_none_active()
        {
            var folders = TemplateProvider.PhysicalTemplateFolders("Templates", null).ToList();
            Assert.Equal(new[] { "Views/Templates", "UI/Templates" }, folders);
        }
    }
}
