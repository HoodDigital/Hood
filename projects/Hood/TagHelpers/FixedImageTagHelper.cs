using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("fixedImage")]
    public class FixedImageTagHelper : TagHelper
    {

        private const string SrcAttrName = "src";
        [HtmlAttributeName(SrcAttrName)]
        public string Src { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "hood-image ");
            }
            else
            {
                classValue = "hood-image";
            }
            output.TagName = "figure";
            output.TagMode = TagMode.StartTagAndEndTag;
            string url = "/lib/hood/images/no-image.jpg";
            if (!string.IsNullOrEmpty(Src))
            {
                url = Src;
            }
            string styleValue;
            if (output.Attributes.ContainsName("style"))
            {
                styleValue = string.Format("{0};{1}", output.Attributes["style"].Value, string.Format("background-image:url({0});", url)); 
            }
            else
            {
                styleValue = string.Format("background-image:url({0});", url);
            }
            output.Content.AppendHtml(output.Content.GetContent() + "<img src='" + url + "' alt='' />");
            output.Attributes.SetAttribute("class", classValue);
            output.Attributes.SetAttribute("style", styleValue);
       }
    }

}
