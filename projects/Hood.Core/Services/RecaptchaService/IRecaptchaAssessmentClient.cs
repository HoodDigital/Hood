using System.Threading.Tasks;
using Google.Cloud.RecaptchaEnterprise.V1;

namespace Hood.Services
{
    /// <summary>
    /// Thin seam over the Google reCAPTCHA Enterprise SDK so that
    /// <see cref="RecaptchaService"/>'s decision logic can be tested
    /// against faked assessments.
    /// </summary>
    internal interface IRecaptchaAssessmentClient
    {
        Task<Assessment> CreateAssessmentAsync(
            string projectId,
            string apiKey,
            Event assessmentEvent
        );
    }
}
