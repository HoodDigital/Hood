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
    [HtmlTargetElement("editor", Attributes = "asp-image")]
    public class MediaSelectTagHelper : TagHelper
    {
        public override int Order { get; } = int.MaxValue;

        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("asp-image")]
        public ModelExpression For { get; set; }
        /// <summary>
        /// Default: form-control
        /// </summary>
        [HtmlAttributeName("asp-input-class")]
        public string InputClass { get; set; } = "form-control";
        /// <summary>
        /// Default: form-control
        /// </summary>
        [HtmlAttributeName("asp-filetype")]
        public GenericFileType FileType { get; set; } = GenericFileType.Image;
        /// <summary>
        /// Default: true
        /// </summary>
        [HtmlAttributeName("asp-floating-label")]
        public bool Floating { get; set; } = true;
        /// <summary>
        /// Default: cover
        /// </summary>
        [HtmlAttributeName("asp-background-size")]
        public string BackgroundSize { get; set; } = "cover";
        /// <summary>
        /// Default: img img-xs border-3 border-white shadow-sm
        /// </summary>
        [HtmlAttributeName("asp-image-class")]
        public string ImageClass { get; set; } = "img img-xs border-3 border-white shadow-sm";
        /// <summary>
        /// Default: fa fa-camera fa-2x
        /// </summary>
        [HtmlAttributeName("asp-icon-class")]
        public string ButtonIcon { get; set; } = "fa fa-camera fa-2x";
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

            var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
            var attachUrl = _urlHelperFactory.GetUrlHelper(ViewContext).Action("Action", "Media", new { area = "Admin", doAction = MediaWindowAction.Select.ToString(), tag = $".{fieldId}", fileType = FileType, restrict = true }); ;

            output.Content.SetHtmlContent($@"
                <div class='row no-gutter align-items-center'>
                    <div class='col-auto pr-0'>
                        <figure class='{ImageClass} {fieldId}' style='background-image:url({fieldValue});background-size:{BackgroundSize};'>
                            <img src='{fieldValue}' alt=''>
                        </figure>                        
                    </div>
                    <div class='col'>
                        <div class='{floatingClass}'>
                            <label for='{fieldName}'>{fieldDisplayName}</label>
                            <input type='url' class='{InputClass} {fieldId}' placeholder='Enter a url...' id='{fieldId}' name='{fieldName}' value='{fieldValue}' />
                        </div>                        
                    </div>
                    <div class='col-auto pl-0'>
                        <button class='btn btn-dark btn-lg hood-media-select' data-url='{attachUrl}' data-target='.{fieldId}' type='button'><i class='{ButtonIcon}'></i></button>
                    </div>
                </div>
                {fieldDescription}
            ");
        }
    }
}
