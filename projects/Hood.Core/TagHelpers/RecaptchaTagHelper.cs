using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("recaptcha")]
    public class RecapcthaTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        public RecapcthaTagHelper(IHtmlHelper htmlHelper, IHttpContextAccessor httpContextAccessor)
        {
            _htmlHelper = htmlHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public override int Order { get; } = int.MaxValue;

        /// <summary>
        /// Set a Font-Awesome Icon here for example "fa-user-friends".
        /// </summary>
        [HtmlAttributeName("action")]
        public string Action { get; set; } = "homepage";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Engine.Settings.Integrations.EnableGoogleRecaptcha)
                return;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            string recaptchaId = Guid.NewGuid().ToString();
            _htmlHelper.AddScript(ResourceLocation.BeforeScripts, $"https://www.google.com/recaptcha/api.js?render={Engine.Settings.Integrations.GoogleRecaptchaSiteKey}", false);
            _htmlHelper.AddScript(ResourceLocation.BeforeScripts, $"https://cdn.jsdelivr.net/npm/hoodcms@4.0.1/js/recaptcha.js", false);
            var scriptTemplate = $@"<script>hood__getReCaptcha('{Engine.Settings.Integrations.GoogleRecaptchaSiteKey}','{recaptchaId}','{Action}');setInterval(function(){{hood__getReCaptcha('{Engine.Settings.Integrations.GoogleRecaptchaSiteKey}','{recaptchaId}','{Action}');}},150000);</script>";
            _htmlHelper.AddInlineScript(ResourceLocation.AfterScripts, scriptTemplate);
            output.Content.SetHtmlContent($@"<input id=""{recaptchaId}"" name=""g-recaptcha-response"" type=""hidden"" value="""" />");
        }
    }
}
