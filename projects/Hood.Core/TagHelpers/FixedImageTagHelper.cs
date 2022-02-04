using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("fixedImage")]
    public class FixedImageTagHelper : TagHelper
    {
        [HtmlAttributeName("alt")]
        public string Alt { get; set; }

        [HtmlAttributeName("src")]
        public string Src { get; set; }

        [HtmlAttributeName("default")]
        public bool UseDefault { get; set; } = true;

        [HtmlAttributeName("fallback")]
        public string Fallback { get; set; }

        [HtmlAttributeName("color")]
        public string Colour { get; set; }

        public FixedImageTagHelper()
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            string styleValue = "";

            if (output.Attributes.ContainsName("style"))
                styleValue += $"{output.Attributes["style"].Value};";

            if (Colour.IsSet())
                styleValue += $"background-color:{Colour};";

            string url = "";

            if (UseDefault)
                if (Engine.Settings.Media.NoImage.IsSet())
                    url = Engine.Settings.Media.NoImage;
                else
                    url = "https://cdn.jsdelivr.net/npm/hoodcms/images/no-image.jpg";

            if (Fallback.IsSet())
                url = Fallback;

            if (Src.IsSet())
                url = Src;

            if (url.IsSet())
                styleValue += $"background-image:url({url});";

            output.Attributes.SetAttribute("style", styleValue);

            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} hood-image");
            else
                output.Attributes.SetAttribute("class", $"hood-image");

            output.PreContent.SetHtmlContent($"<img src='{url}' alt='{Alt}' />");
        }
    }
}
