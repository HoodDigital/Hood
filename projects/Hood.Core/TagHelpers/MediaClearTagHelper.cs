using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
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
            output.Attributes.Add("data-hood-media", "clear");

            if (!output.Attributes.ContainsName("href") && Url.IsSet())
            {
                output.Attributes.Add("href", Url);
            }

            if (Refresh.IsSet())
            {
                output.Attributes.Add("data-hood-media-refresh", Refresh);
            }
        }
    }

}
