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
        public string ImageClass { get; set; } = "img img-xs border-3 border-white shadow-sm";
        /// <summary>
        /// Default: img img-xs border-3 border-white shadow-sm
        /// </summary>
        [HtmlAttributeName("asp-pickr-class")]
        public string PickerClass { get; set; } = "pickr w-100 h-100";
        /// <summary>
        /// Default: true
        /// </summary>
        [HtmlAttributeName("asp-floating-label")]
        public bool Floating { get; set; } = true;
        /// <summary>
        /// Default: form-group image-editor row no-gutter align-items-center
        /// </summary>
        [HtmlAttributeName("asp-group-class")]
        public string GroupClass { get; set; } = "form-group image-editor";

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

            string fieldName = For.Name;

            string fieldDisplayName = For.Name;
            if (For.ModelExplorer.Metadata.DisplayName.IsSet())
                fieldDisplayName = For.ModelExplorer.Metadata.DisplayName;

            string fieldDescription = "";
            if (For.ModelExplorer.Metadata.Description.IsSet())
                fieldDescription = $"<small class='form-text text-info'>{For.ModelExplorer.Metadata.Description}</small>";

            string fieldId = Guid.NewGuid().ToString();

            string fieldValue = For.Model != null ? For.Model.ToString() : "";

            string floatingClass = Floating ? "floating-label" : "";

            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} {GroupClass}");
            else
                output.Attributes.SetAttribute("class", $"{GroupClass}");

            output.Content.SetHtmlContent($@"
                <div class='row no-gutter align-items-center'>
                    <div class='col-auto pr-0'>
                        <figure class='{ImageClass} color-picker' data-target='#{fieldId}'>
                            <div class='{PickerClass}'></div>
                        </figure>                        
                    </div>
                    <div class='col'>
                        <div class='{floatingClass}'>
                            <label for='{fieldName}'>{fieldDisplayName}</label>
                            <input type='text' class='{InputClass}' placeholder='Choose a colour...' id='{fieldId}' name='{fieldName}' value='{fieldValue}' />
                        </div>                        
                    </div>
                </div>
                {fieldDescription}
            ");
        }
    }
}
