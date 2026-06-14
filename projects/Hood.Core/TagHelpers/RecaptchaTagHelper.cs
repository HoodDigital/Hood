using System;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("recaptcha")]
    public class RecaptchaTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        public RecaptchaTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public override int Order { get; } = int.MaxValue;

        /// <summary>
        /// The recaptcha action this form reports to Google, e.g. "login" or "register".
        /// Pass the same value as expectedAction when validating server-side.
        /// </summary>
        [HtmlAttributeName("action")]
        public string Action { get; set; } = "homepage";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Integrations is null when the IntegrationSettings option row doesn't exist yet (a site
            // whose settings have never been saved / a partially-installed database). Treat that — and
            // the recaptcha-disabled case — identically: render nothing rather than NRE the page.
            if (Engine.Settings?.Integrations?.EnableGoogleRecaptcha != true)
                return;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            string recaptchaId = Guid.NewGuid().ToString();
            _htmlHelper.AddScript(
                ResourceLocation.BeforeScripts,
                $"https://www.google.com/recaptcha/enterprise.js?render={Engine.Settings.Integrations.GoogleRecaptchaSiteKey}"
            );
            _htmlHelper.AddInlineScript(
                ResourceLocation.BeforeScripts,
                $"<script>function hood__getReCaptcha(key, recaptchaId, action) {{grecaptcha.enterprise.ready(function() {{grecaptcha.enterprise.execute(key, {{ 'action': action }}).then(function(token) {{document.getElementById(recaptchaId).value = token;}}).catch(function(e) {{console.error('reCAPTCHA could not run — check the site key is a reCAPTCHA Enterprise key for this domain.', e);}});}});}}</script>"
            );
            var scriptTemplate =
                $@"<script>hood__getReCaptcha('{Engine.Settings.Integrations.GoogleRecaptchaSiteKey}','{recaptchaId}','{Action}');setInterval(function(){{hood__getReCaptcha('{Engine.Settings.Integrations.GoogleRecaptchaSiteKey}','{recaptchaId}','{Action}');}},150000);</script>";
            _htmlHelper.AddInlineScript(ResourceLocation.AfterScripts, scriptTemplate);
            output.Content.SetHtmlContent(
                $@"<input id=""{recaptchaId}"" name=""g-recaptcha-response"" type=""hidden"" value="""" />"
            );
        }
    }
}
