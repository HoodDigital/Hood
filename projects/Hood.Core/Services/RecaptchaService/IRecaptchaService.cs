using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IRecaptchaService
    {
        /// <summary>
        /// Validates the recaptcha token posted with the request via the Google Fraud Defence
        /// assessment API. When <paramref name="expectedAction"/> is supplied, the token's
        /// action must match it — pass the action used by the form's recaptcha tag.
        /// </summary>
        System.Threading.Tasks.Task<RecaptchaResponse> Validate(
            HttpRequest request,
            string expectedAction = null
        );
    }
}
