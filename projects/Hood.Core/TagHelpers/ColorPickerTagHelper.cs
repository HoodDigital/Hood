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

            // Attempt to check for a Placeholder annotation
            string fieldName = For.Name;

            if (For.ModelExplorer.Metadata.DisplayName.IsSet())
            {
                fieldName = For.ModelExplorer.Metadata.DisplayName;
            }


            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} row align-items-center mb-3");
            else
                output.Attributes.SetAttribute("class", $"row align-items-center mb-3");

            string customAttributes = "";
            foreach (var attribute in output.Attributes)
            {
                if (attribute.Name.StartsWith("data-"))
                {
                    customAttributes += $"{attribute.Name}='{attribute.Value}'";
                }
            }

            output.Content.SetHtmlContent($@"
                <div class='col-auto' style='width:75px;'>
                    <div class='img img-full img-square img-circle color-picker shadow' 
                         data-target='#{fieldId}' {customAttributes}>
                        <div class='pickr'></div>
                    </div>                
                </div>
                <div class='col'>
                    <div class='form-floating'>
                        <input id='{fieldId}'
                               name='{For.Name}'
                               value='{fieldValue}'
                               placeholder='{fieldName}'
                               class='form-control' />
                        <label for='{fieldId}'>{fieldName}</label>
                    </div>
                </div>
            ");
        }
    }
}
