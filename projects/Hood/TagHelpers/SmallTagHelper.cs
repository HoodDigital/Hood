using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("small", Attributes = "asp-for")]
    public class FormTextTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (For.ModelExplorer.Metadata.Description.IsSet())
            {
                output.PreContent.SetHtmlContent($"{For.ModelExplorer.Metadata.Description}");
            }
            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value}");
            else
                output.Attributes.SetAttribute("class", $"form-text text-info");
        }
    }
}
