using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    [HtmlTargetElement("textarea", Attributes = "asp-for")]
    public class PlaceholderTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (For.Metadata.IsRequired)
            {
                if (context.AllAttributes["required"] == null)
                {
                    output.Attributes.Add("required", "required");
                }
                else
                {
                    output.Attributes.SetAttribute("required", "required");
                }
            }

            // Process only if 'Process' attribute is not present already
            if (context.AllAttributes["placeholder"] == null)
            {
                // Attempt to check for a Placeholder annotation
                if (For.ModelExplorer.Metadata.DisplayName.IsSet())
                {
                    output.Attributes.Add("placeholder", For.ModelExplorer.Metadata.DisplayName);
                }
                else
                {
                    output.Attributes.Add("placeholder", For.Name);
                }
            }
        }
    }
}
