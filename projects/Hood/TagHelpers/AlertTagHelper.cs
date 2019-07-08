using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("alert")]
    public class AlertTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        /// <summary>
        /// Choose the type of alert to display
        /// </summary>
        [HtmlAttributeName("type")]
        public AlertType Type { get; set; }
        /// <summary>
        /// Choose a size for the alert.
        /// </summary>
        [HtmlAttributeName("size")]
        public AlertSize Size { get; set; }
        /// <summary>
        /// Set a Font-Awesome Icon here for example "fa-user-friends".
        /// </summary>
        [HtmlAttributeName("icon")]
        public string Icon { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            string alertTemplate = (await output.GetChildContentAsync()).GetContent();

            if (alertTemplate.IsSet())
            {
                string iconTemplate;
                string alertClass;
                switch (Size)
                {
                    case AlertSize.Small:
                    default:
                        iconTemplate = Icon.IsSet() ? $"<div class='p-1'><i class='{Icon} {Size.ToIconSizeCssClass()}'></i></div>" : "";
                        alertTemplate = $"{iconTemplate}<div class='p-1 flex-grow-1'>{alertTemplate}</div>";
                        alertClass = "d-flex flex-row align-items-center p-1";
                        break;
                    case AlertSize.Medium:
                        iconTemplate = Icon.IsSet() ? $"<div class='p-2'><i class='{Icon} {Size.ToIconSizeCssClass()} mr-2'></i></div>" : "";
                        alertTemplate = $"{iconTemplate}<div class='p-2'>{alertTemplate}</div>";
                        alertClass = "d-flex flex-row align-items-center p-2";
                        break;
                    case AlertSize.Large:
                        iconTemplate = Icon.IsSet() ? $"<div class='p-2'><i class='{Icon} {Size.ToIconSizeCssClass()} mr-2'></i></div>" : "";
                        alertTemplate = $"{iconTemplate}<div class='p-2'>{alertTemplate}</div>";
                        alertClass = "d-flex flex-row align-items-center p-2";
                        break;
                    case AlertSize.Epic:
                        iconTemplate = Icon.IsSet() ? $"<div class='p-3'><i class='{Icon} {Size.ToIconSizeCssClass()}'></i></div>" : "";
                        alertTemplate = $"{iconTemplate}<div class='p-3'>{alertTemplate}</div>";
                        alertClass = "d-flex flex-column p-3";
                        break;
                }

                if (output.Attributes.ContainsName("class"))
                    output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} alert {Type.ToAlertCssClass()} {alertClass}");
                else
                    output.Attributes.SetAttribute("class", $"alert {Type.ToAlertCssClass()} {alertClass}");

                output.Content.SetHtmlContent(alertTemplate);
            }
        }
    }
}
