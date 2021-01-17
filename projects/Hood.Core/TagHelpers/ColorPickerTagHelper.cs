using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("colorSelect")]
    public class ColorPickerTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Default: form-control
        /// </summary>
        [HtmlAttributeName("asp-input-class")]
        public string InputClass { get; set; } = "form-control";

        /// <summary>
        /// Default: img img-xs border-3 border-white shadow-sm
        /// </summary>
        [HtmlAttributeName("asp-image-class")]
        public string ImageClass { get; set; } = "img img-xs m-0 border-3 border-white shadow-sm";

        /// <summary>
        /// Default: img img-xs border-3 border-white shadow-sm
        /// </summary>
        [HtmlAttributeName("asp-pickr-class")]
        public string PickerClass { get; set; } = "pickr w-100 h-100";

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            string fieldId = Guid.NewGuid().ToString();

            string fieldValue = For.Model != null ? For.Model.ToString() : "";

            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} row no-gutter align-items-center");
            else
                output.Attributes.SetAttribute("class", $"row no-gutter align-items-center");

            string customAttributes = "";
            foreach (var attribute in output.Attributes)
            {
                if (attribute.Name.StartsWith("data-"))
                {
                    customAttributes += $"{attribute.Name}='{attribute.Value}'";
                }
            }

            output.Content.SetHtmlContent($@"
                <div class='col-auto pr-0'>
                    <figure class='{ImageClass} color-picker' data-target='#{fieldId}' {customAttributes}>
                        <div class='{PickerClass}'></div>
                    </figure>                        
                </div>
                <div class='col'>
                    <input type='text' class='{InputClass}' placeholder='Choose a colour...' id='{fieldId}' name='{For.Name}' value='{fieldValue}' />
                </div>
            ");
        }
    }
}
