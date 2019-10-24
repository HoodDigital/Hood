using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("li", Attributes = "asp-for")]
    public class DataListItemTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        [HtmlAttributeName("asp-list-item-class")]
        public string ListItemClass { get; set; } = "list-group-item flex-column flex-md-row d-flex justify-content-between align-items-centent";
        [HtmlAttributeName("asp-label-class")]
        public string LabelClass { get; set; } = "col-md-3 p-0";
        [HtmlAttributeName("asp-value-class")]
        public string Valueclass { get; set; } = "col-md text-md-right p-0 pl-md-3";
        [HtmlAttributeName("asp-multiline")]
        public bool Multiline { get; set; } = false;


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            string fieldValue = "";
            if (For.Model != null)
            {
                switch (For.Metadata.ModelType.Name)
                {
                    case nameof(DateTime):
                        fieldValue = ((DateTime)For.Model).ToDisplay();
                        break;
                    default:
                        fieldValue = For.Model.ToString();
                        break;
                }
            }

            string fieldName = For.ModelExplorer.Metadata.Name.IsSet() ? For.Name : "";
            string truncate = Multiline ? "" : "text-truncate";

            output.PreContent.SetHtmlContent($"<strong class='{LabelClass}'>{fieldName}</strong><span class='{Valueclass} {truncate}'>{fieldValue}</span>");

            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} {ListItemClass}");
            else
                output.Attributes.SetAttribute("class", $" {ListItemClass}");

            output.Attributes.SetAttribute("title", $" {fieldValue}");
        }
    }
}
