using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("select", Attributes = "asp-for")]
    [HtmlTargetElement("input", Attributes = "asp-for")]
    [HtmlTargetElement("textarea", Attributes = "asp-for")]
    public class RequiredTagHelper : TagHelper
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
        }
    }
}
