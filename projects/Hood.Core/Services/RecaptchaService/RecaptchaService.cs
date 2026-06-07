using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Google.Cloud.RecaptchaEnterprise.V1;
using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    /// <summary>
    /// Validates recaptcha tokens via the Google Fraud Defence (reCAPTCHA Enterprise)
    /// assessment API. The legacy siteverify protocol is not supported — keys must be
    /// migrated to the new platform and paired with a project id + API key.
    /// </summary>
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IRecaptchaAssessmentClient _assessmentClient;
        private readonly System.Func<Models.IntegrationSettings> _settingsSource;

        public RecaptchaService()
            : this(new RecaptchaAssessmentClient(), () => Engine.Settings.Integrations) { }

        // Test seam — lets Hood.Tests fake the Google assessment call and the
        // settings store without bootstrapping the Engine singleton.
        internal RecaptchaService(
            IRecaptchaAssessmentClient assessmentClient,
            System.Func<Models.IntegrationSettings> settingsSource
        )
        {
            _assessmentClient = assessmentClient;
            _settingsSource = settingsSource;
        }

        public async Task<RecaptchaResponse> Validate(
            HttpRequest request,
            string expectedAction = null
        )
        {
            try
            {
                Models.IntegrationSettings settings = _settingsSource();

                // Only enforce recaptcha when it is fully configured (toggle on AND site key,
                // project id and API key all set). Otherwise treat the check as passed so
                // login / register / forms aren't blocked — callers gate on .Passed, so this
                // must set Passed, not just Success.
                if (!settings.IsGoogleRecaptchaEnabled)
                    return new RecaptchaResponse() { Success = true, Passed = true };

                if (!request.Form.ContainsKey("g-recaptcha-response")) // error if no reason to do anything, this is to alert developers they are calling it without reason.
                {
                    throw new ValidationException(
                        "Google recaptcha response not found in form. Did you forget to include it?"
                    );
                }

                string token = request.Form["g-recaptcha-response"];
                Assessment assessment = await _assessmentClient.CreateAssessmentAsync(
                    settings.GoogleRecaptchaProjectId,
                    settings.GoogleRecaptchaApiKey,
                    new Event
                    {
                        Token = token ?? string.Empty,
                        SiteKey = settings.GoogleRecaptchaSiteKey,
                        ExpectedAction = expectedAction ?? string.Empty,
                    }
                );

                if (assessment?.TokenProperties == null)
                {
                    throw new ValidationException("Recaptcha assessment returned no token data.");
                }

                if (!assessment.TokenProperties.Valid)
                {
                    throw new ValidationException(
                        $"Recaptcha token invalid: {assessment.TokenProperties.InvalidReason}."
                    );
                }

                if (expectedAction.IsSet() && assessment.TokenProperties.Action != expectedAction)
                {
                    throw new ValidationException(
                        "Recaptcha action and expected action do not match. Forgery attempt?"
                    );
                }

                if (assessment.TokenProperties.Hostname?.ToLower() != request.Host.Host?.ToLower())
                {
                    throw new ValidationException(
                        "Recaptcha host, and request host do not match. Forgery attempt?"
                    );
                }

                decimal score = (decimal)(assessment.RiskAnalysis?.Score ?? 0);
                if (score < settings.GoogleRecaptchaThreshold)
                {
                    throw new ValidationException("Recaptcha failed to pass security threshold.");
                }

                // Reached here? Fairly sure we are golden, mark as passed and return.
                return new RecaptchaResponse()
                {
                    Success = true,
                    Passed = true,
                    Score = score,
                    Action = assessment.TokenProperties.Action,
                    HostName = assessment.TokenProperties.Hostname,
                    ChallengeTS = assessment.TokenProperties.CreateTime?.ToDateTime() ?? default,
                };
            }
            catch (ValidationException ex)
            {
                return new RecaptchaResponse(false, ex.Message);
            }
        }
    }
}
