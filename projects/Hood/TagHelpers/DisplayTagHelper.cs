using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("display")]
    public class DisplayTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        /// <summary>
        /// Choose the type of alert to display
        /// </summary>
        [HtmlAttributeName("type")]
        public AlertType Type { get; set; }
        /// <summary>
        /// Choose a size for the alert.
        /// </summary>
        [HtmlAttributeName("multiline")]
        public bool Multiline { get; set; }
        /// <summary>
        /// Set a Font-Awesome Icon here for example "fa-user-friends".
        /// </summary>
        [HtmlAttributeName("icon")]
        public string Icon { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            string fieldId = Guid.NewGuid().ToString();

            string fieldDisplayName = For.Name;
            if (For.ModelExplorer.Metadata.DisplayName.IsSet())
                fieldDisplayName = For.ModelExplorer.Metadata.DisplayName;

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

            string template = "";

            if (Multiline)
            {
                template += $@"<textarea id='{fieldId}' class='form-control select-text disabled' placeholder='{For.ModelExplorer.Metadata.DisplayName}' disabled>{fieldValue}";
            }
            else
            {
                template += $@"<input id='{fieldId}' class='form-control select-text disabled' placeholder='{For.ModelExplorer.Metadata.DisplayName}' value='{fieldValue}' disabled />";
            }

            template += $@"<label for='{fieldId}'>{fieldDisplayName}</label>";

            if (For.ModelExplorer.Metadata.Description.IsSet())
                template += $"<small class='form-text text-info'>{For.ModelExplorer.Metadata.Description}</small>";

            if (!output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"form-group floating-label");

            output.Content.SetHtmlContent(template);
        }
    }
}
