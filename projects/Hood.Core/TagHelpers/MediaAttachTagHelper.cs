using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

        /// <summary>
        /// The json field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("media-json")]
        public ModelExpression Json { get; set; }

        [HtmlAttributeName("media-list")]
        public string List { get; set; }

        [HtmlAttributeName("media-refresh")]
        public string Refresh { get; set; }

        [HtmlAttributeName("media-filetype")]
        public GenericFileType FileType { get; set; } = GenericFileType.Image;

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
            output.Attributes.Add("data-hood-media-filetype", FileType);

            if (!List.IsSet())
            {
                var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
                List = _urlHelperFactory.GetUrlHelper(ViewContext).Action("Action", "Media", new{ area = "Admin" });
            }
            output.Attributes.Add("data-hood-media-list", List);

            if (Json != null)
            {
                output.Attributes.Add("data-hood-media-json", Json.Name);
            }
            if (Refresh.IsSet())
            {
                output.Attributes.Add("data-hood-media-refresh", Refresh);
            }
        }
    }

    [HtmlTargetElement("a", Attributes = "media-clear")]
    [HtmlTargetElement("button", Attributes = "media-clear")]
    public class MediaClearTagHelper : TagHelper
    {
        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("media-clear")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("media-url")]
        public string Url { get; set; }

        /// <summary>
        /// The json field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("media-json")]
        public ModelExpression Json { get; set; }

        [HtmlAttributeName("media-refresh")]
        public string Refresh { get; set; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("data-hood-media", "clear");

            if (Json != null)
            {
                output.Attributes.Add("data-hood-media-json", Json.Name);
            }
            if (Refresh.IsSet())
            {
                output.Attributes.Add("data-hood-media-refresh", Refresh);
            }
        }
    }

}
