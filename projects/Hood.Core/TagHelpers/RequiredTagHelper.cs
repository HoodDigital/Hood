using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

            var property = For.Metadata
                            .ContainerType
                            .GetProperty(For.Metadata.Name);

            var required = Attribute.IsDefined(property, typeof(RequiredAttribute));

            if (required)
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
            else
            {
                if (output.Attributes.ContainsName("required"))
                {
                    output.Attributes.RemoveAll("required");
                }
            }
        }
    }
}
