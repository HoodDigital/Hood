using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "media-gallery")]
    [HtmlTargetElement("button", Attributes = "media-gallery")]
    public class MediaGalleryTagHelper : TagHelper
    {
        /// <summary>
        /// HTML id for the gallery display list
        /// </summary>
        [HtmlAttributeName("media-gallery")]
        public string Gallery { get; set; }

        [HtmlAttributeName("media-list")]
        public string List { get; set; }

        [HtmlAttributeName("media-url")]
        public string Url { get; set; }

        [HtmlAttributeName("media-refresh")]
        public string Refresh { get; set; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("data-hood-media", "gallery");
            output.Attributes.Add("data-hood-media-url", Url);
            output.Attributes.Add("data-hood-media-target", Gallery);

            if (!List.IsSet())
            {
                var _urlHelperFactory = Engine.Services.Resolve<IUrlHelperFactory>();
                List = _urlHelperFactory.GetUrlHelper(ViewContext).Action("Action", "Media", new { area = "Admin" });
            }
            output.Attributes.Add("data-hood-media-list", List);


            if (Refresh.IsSet())
            {
                output.Attributes.Add("data-hood-media-refresh", Refresh);
            }
        }
    }

}
