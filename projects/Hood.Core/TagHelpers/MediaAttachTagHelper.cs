using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "media-attach")]
    [HtmlTargetElement("button", Attributes = "media-attach")]
    [HtmlTargetElement("div", Attributes = "media-attach")]
    [HtmlTargetElement("img", Attributes = "media-attach")]
    [HtmlTargetElement("input", Attributes = "media-attach")]
    public class MediaAttachTagHelper : TagHelper
    {
        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("media-attach")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("media-url")]
        public string Url { get; set; }

        [HtmlAttributeName("media-list")]
        public string List { get; set; }

        [HtmlAttributeName("media-refresh")]
        public string Refresh { get; set; }

        [HtmlAttributeName("media-types")]
        public string Types { get; set; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("data-hood-media", "attach");
            output.Attributes.Add("data-hood-media-url", Url);
            output.Attributes.Add("data-hood-media-types", Types);

            if (!List.IsSet())
            {
                var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
                List = _urlHelperFactory.GetUrlHelper(ViewContext).Action("Action", "Media", new{ area = "Admin" });
            }
            output.Attributes.Add("data-hood-media-list", List);

            if (Refresh.IsSet())
            {
                output.Attributes.Add("data-hood-media-refresh", Refresh);
            }
        }
    }

    [HtmlTargetElement("div", Attributes = "media-select")]
    public class MediaSelectTagHelper : TagHelper
    {
        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("media-select")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("media-url")]
        public string Url { get; set; }

        [HtmlAttributeName("media-list")]
        public string List { get; set; }

        [HtmlAttributeName("media-refresh")]
        public string Refresh { get; set; }

        [HtmlAttributeName("media-types")]
        public string Types { get; set; }

        [HtmlAttributeName("media-size")]
        public string Size { get; set; } = "large";

        [HtmlAttributeName("media-fit")]
        public string Fit { get; set; } = "cover";

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            string fieldName = For.Name;

            string fieldDisplayName = For.Name;
            if (For.ModelExplorer.Metadata.DisplayName.IsSet())
                fieldDisplayName = For.ModelExplorer.Metadata.DisplayName;

            string fieldDescription = "";
            if (For.ModelExplorer.Metadata.Description.IsSet())
                fieldDescription = $"<small class='form-text text-info'>{For.ModelExplorer.Metadata.Description}</small>";

            string fieldId = Guid.NewGuid().ToString();

            string fieldValue = For.Model != null ? For.Model.ToString() : "";

            if (!List.IsSet())
            {
                var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
                List = _urlHelperFactory.GetUrlHelper(ViewContext).Action("Action", "Media", new { area = "Admin" });
            }

            var template = $@"
<div class='image-editor mb-3'>
    <div class='row align-items-center'>
        <div class='col-auto' style='width:75px;'>
            <div class='img img-full img-square img-circle bg-light shadow {fieldId}' style='background-image:url({fieldValue});background-size:{Fit};'></div>
        </div>
        <div class='col'>
            <div class='form-floating'>
                <input type='url' class='form-control'
                        placeholder='{fieldDisplayName}'
                        id='{fieldId}'
                        name='{fieldName}'
                        value='{fieldValue}'>
                <label for='Logo'>{fieldDisplayName}</label>
            </div>
        </div>
        <div class='col-auto pl-0'>
            <button class='btn btn-dark btn-lg'
                    data-hood-media='select'
                    data-hood-media-size='{Size}'
                    data-hood-media-list='{List}'
                    data-hood-media-types='{Types}'
                    data-hood-media-target='#{fieldId}'
                    data-hood-media-refresh='.{fieldId}'
                    type='button'>
                <i class='fa fa-camera fa-2x'></i>
            </button>
        </div>
    </div>
</div>";

            output.Content.SetHtmlContent(template);

        }
    }

}
