using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("script", Attributes = "asp-location")]
    public class ScriptTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Indicates where the script should be rendered
        /// </summary>
        [HtmlAttributeName("asp-location")]
        public ResourceLocation Location { set; get; }
        /// <summary>
        /// Indicates where the script should be rendered
        /// </summary>
        [HtmlAttributeName("asp-bundle")]
        public bool Bundle { set; get; } = false;

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        public ScriptTagHelper(IHtmlHelper htmlHelper, IHttpContextAccessor httpContextAccessor)
        {
            _htmlHelper = htmlHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //get JavaScript
            var script = output.GetChildContentAsync().Result.GetContent();

            //build script tag
            var scriptTag = new TagBuilder("script");
            scriptTag.InnerHtml.SetHtmlContent(new HtmlString(script));

            //merge attributes
            foreach (var attribute in context.AllAttributes)
                if (!attribute.Name.StartsWith("asp-"))
                    scriptTag.Attributes.Add(attribute.Name, attribute.Value.ToString());

            if (context.AllAttributes.ContainsName("src"))
            {
                var src = context.AllAttributes["src"].Value.ToString();
                _htmlHelper.AddScriptParts(Location, src, !Bundle, context.AllAttributes.ContainsName("async"), context.AllAttributes.ContainsName("defer"));
            }
            else
            {
                _htmlHelper.AddInlineScriptParts(Location, scriptTag.RenderHtmlContent());
            }

            //generate nothing
            output.SuppressOutput();
        }
    }


}
