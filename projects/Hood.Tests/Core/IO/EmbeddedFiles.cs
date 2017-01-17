using Xunit;

namespace Hood.Tests.Core.IO
{
    public class EmbeddedFiles
    {
        [Fact]
        public void GetFiles()
        {
            string[] files = {
                "_Contact.cshtml",
                "_Demo.cshtml",
                "_Page.cshtml"
            };
            var actual = Hood.IO.EmbeddedFiles.GetFiles("~/Views/Templates/");
            Assert.Equal(files, actual);
        }

        [Theory]
        [InlineData("~/Views/Templates/_Contact.cshtml", "Views.Templates._Contact.cshtml", false)]
        [InlineData("~/Views/Templates/_Demo.cshtml", "Views.Templates._Demo.cshtml", false)]
        [InlineData("~/Views/SliderTemplates/_OwlCarouselDefault.cshtml", "Views.SliderTemplates._OwlCarouselDefault.cshtml", false)]
        [InlineData("~/Areas/Admin/Views/Property/_Editor_Descriptions.cshtml", "Areas.Admin.Views.Property._Editor_Descriptions.cshtml", false)]
        [InlineData("~/Views/Templates/Extras", "Views.Templates.Extras.", true)]
        [InlineData("~/Views/Templates/", "Views.Templates.", true)]
        [InlineData("~/Views/SliderTemplates", "Views.SliderTemplates.", true)]
        [InlineData("~/Views/SliderTemplates/", "Views.SliderTemplates.", true)]
        [InlineData("~/Areas/Admin/Views/Property/", "Areas.Admin.Views.Property.", true)]
        public void ReWritePath(string value, string expected, bool directory)
        {
            var actual = Hood.IO.EmbeddedFiles.ReWritePath(value, directory);
            Assert.Equal(actual, expected);
        }
    }
}
