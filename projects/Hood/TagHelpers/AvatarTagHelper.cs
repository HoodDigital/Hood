using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("avatar", Attributes = AvatarBlobAttributeNName)]
    public class AvatarTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string AvatarBlobAttributeNName = "blob";

        [HtmlAttributeName(AvatarBlobAttributeNName)]
        public string Blob { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "hood-avatar");
            }
            else
            {
                classValue = "hood-avatar";
            }
            output.Attributes.SetAttribute("class", classValue);
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            string url = "/images/no-avatar.jpg";
            if (!string.IsNullOrEmpty(Blob))
            {
                url = "https://hood.blob.core.windows.net/" + Blob.ToString();
            }
            output.Attributes.SetAttribute("style", string.Format("background-image:url({0});", url));
            output.Content.AppendHtml("<img src='" + url + "' alt='Avatar' />");

        }
    }

}
