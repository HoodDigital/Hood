using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Hood.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        public static bool UseRecaptchaNet { get; set; } = false;

        public async Task<RecaptchaResponse> Validate(HttpRequest request)
        {
            try
            {
                Models.IntegrationSettings settings = Engine.Settings.Integrations;

                // Only enforce recaptcha when it is fully configured (toggle on AND both keys set).
                // Otherwise treat the check as passed so login / register / forms aren't blocked —
                // callers gate on .Passed, so this must set Passed, not just Success.
                if (!settings.IsGoogleRecaptchaEnabled)
                    return new RecaptchaResponse() { Success = true, Passed = true };

                if (!request.Form.ContainsKey("g-recaptcha-response")) // error if no reason to do anything, this is to alert developers they are calling it without reason.
                {
                    throw new ValidationException(
                        "Google recaptcha response not found in form. Did you forget to include it?"
                    );
                }

                string domain = UseRecaptchaNet ? "www.recaptcha.net" : "www.google.com";
                string response = request.Form["g-recaptcha-response"];

                HttpClient client = new HttpClient();
                string result = await client.GetStringAsync(
                    $"https://{domain}/recaptcha/api/siteverify?secret={settings.GoogleRecaptchaSecretKey}&response={response}"
                );
                RecaptchaResponse captchaResponse =
                    JsonConvert.DeserializeObject<RecaptchaResponse>(result);
                client.Dispose();

                if (!captchaResponse.Success)
                {
                    throw new ValidationException("Recaptcha failed to validate.");
                }

                if (captchaResponse.HostName?.ToLower() != request.Host.Host?.ToLower())
                {
                    throw new ValidationException(
                        "Recaptcha host, and request host do not match. Forgery attempt?"
                    );
                }

                if (captchaResponse.Score < Engine.Settings.Integrations.GoogleRecaptchaThreshold)
                {
                    throw new ValidationException("Recaptcha failed to pass security threshold.");
                }

                // Reached here? Fairly sure we are golden, marke as passed and return.
                captchaResponse.Passed = true;
                return captchaResponse;
            }
            catch (ValidationException ex)
            {
                return new RecaptchaResponse(false, ex.Message);
            }
        }
    }
}
