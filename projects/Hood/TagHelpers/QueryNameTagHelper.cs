using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    [HtmlTargetElement("select", Attributes = "asp-for")]
    [HtmlTargetElement("textarea", Attributes = "asp-for")]
    public class QueryNameTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            // Process only if 'maxlength' attribute is not present already
            if (For.ModelExplorer.Metadata.BinderModelName.IsSet())
            {
                output.Attributes.SetAttribute("name", $"{For.ModelExplorer.Metadata.BinderModelName}");
            }
        }
    }
}
