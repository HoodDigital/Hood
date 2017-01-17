using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.Core.TagHelpers
{
    [HtmlTargetElement("blobImage", Attributes = BlobAttributeNName)]
    public class BlobImageTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string BlobAttributeNName = "blob";

        [HtmlAttributeName(BlobAttributeNName)]
        public string Blob { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"], "hood-");
            }
            else
            {
                classValue = "progress";
            }
            output.Attributes.SetAttribute("class", classValue);
            output.Attributes.SetAttribute("src", "https://hood.blob.core.windows.net/" + Blob.ToString());
        }
    }

}
