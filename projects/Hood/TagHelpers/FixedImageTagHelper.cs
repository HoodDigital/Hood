﻿using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("fixedImage")]
    public class FixedImageTagHelper : TagHelper
    {
        private const string AltAttrName = "alt";
        [HtmlAttributeName(AltAttrName)]
        public string Alt { get; set; }

        private const string SrcAttrName = "src";
        [HtmlAttributeName(SrcAttrName)]
        public string Src { get; set; }

        private const string DefaultAttrName = "default";
        [HtmlAttributeName(DefaultAttrName)]
        public bool UseDefault { get; set; } = true;

        private const string FallbackAttrName = "fallback";
        [HtmlAttributeName(FallbackAttrName)]
        public string Fallback { get; set; }

        private const string ColourAttrName = "color";
        [HtmlAttributeName(ColourAttrName)]
        public string Colour { get; set; }

        private ISettingsRepository _settings;

        public FixedImageTagHelper(ISettingsRepository settings)
        {
            _settings = settings;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var mediaSettings = _settings.GetMediaSettings();

            output.TagName = "figure";
            output.TagMode = TagMode.StartTagAndEndTag;

            string styleValue = "";

            if (output.Attributes.ContainsName("style"))
                styleValue += $"{output.Attributes["style"].Value};";

            if (Colour.IsSet())
                styleValue += $"background-color:{Colour};";

            string url = "";

            if (UseDefault)
                if (mediaSettings.NoImage.IsSet())
                    url = mediaSettings.NoImage;
                else
                    url = "/lib/hood/images/no-image.jpg";

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
