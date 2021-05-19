using Hood.Core;
using Hood.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("mediaAttach")]
    public class MediaAttachTagHelper : TagHelper
    {
        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        /// <summary>
        /// The json field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("asp-json")]
        public ModelExpression Json { get; set; }

        [HtmlAttributeName("asp-id")]
        public string Id { get; set; }

        [HtmlAttributeName("asp-entity")]
        public string Entity { get; set; }

        /// <summary>
        /// Default: form-control
        /// </summary>
        [HtmlAttributeName("asp-filetype")]
        public GenericFileType FileType { get; set; } = GenericFileType.Image;

        /// <summary>
        /// Default: btn btn-sm btn-info mr-1
        /// </summary>
        [HtmlAttributeName("asp-attach-class")]
        public string AttachButtonClass { get; set; } = "btn btn-sm btn-info mr-1";
        /// <summary>
        /// Default: btn btn-sm btn-info mr-1
        /// </summary>
        [HtmlAttributeName("asp-delete-class")]
        public string DeleteButtonClass { get; set; } = "btn btn-sm btn-danger mr-1";

        [HtmlAttributeName("asp-area")]
        public string Area { get; set; } = "Admin";
        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; } = "Media";
        [HtmlAttributeName("asp-action")]
        public string Action { get; set; } = "Action";
        [HtmlAttributeName("asp-clear")]
        public string Clear { get; set; } = "Clear";

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            string contentTemplate = (await output.GetChildContentAsync()).GetContent();

            var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
            var attachUrl = _urlHelperFactory.GetUrlHelper(ViewContext).Action(Action, Controller, new
            {
                area = Area,
                doAction = MediaWindowAction.Attach.ToString(),
                entity = Entity,
                field = For.Name,
                id = Id,
                fileType = FileType
            });
            var attachButton = $"<a class='{AttachButtonClass} hood-image-attach' data-url='{attachUrl}' data-tag='.{For.Name}' data-json='#{Json.Name}' href='javascript:void(0);'>{contentTemplate}</a>";
            output.Content.SetHtmlContent(attachButton);

            var clearUrl = _urlHelperFactory.GetUrlHelper(ViewContext).Action(Clear, Controller, new
            {
                area = Area,
                entity = Entity,
                field = For.Name,
                id = Id
            });
            var clearButton = $"<a class='{DeleteButtonClass} hood-image-clear' data-url='{clearUrl}' data-tag='.{For.Name}' data-json='#{Json.Name}' href='javascript:void(0);'><i class='fa fa-trash'></i></a>";
            output.PostContent.SetHtmlContent(clearButton);
        }
    }
}
