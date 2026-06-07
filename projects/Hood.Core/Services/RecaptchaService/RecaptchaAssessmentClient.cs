using System.Threading.Tasks;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.RecaptchaEnterprise.V1;

namespace Hood.Services
{
    /// <summary>
    /// Default <see cref="IRecaptchaAssessmentClient"/> — calls the Google Fraud Defence
    /// (reCAPTCHA Enterprise) assessment API, authenticated by API key.
    /// </summary>
    internal class RecaptchaAssessmentClient : IRecaptchaAssessmentClient
    {
        public async Task<Assessment> CreateAssessmentAsync(
            string projectId,
            string apiKey,
            Event assessmentEvent
        )
        {
            RecaptchaEnterpriseServiceClient client =
                await new RecaptchaEnterpriseServiceClientBuilder { ApiKey = apiKey }.BuildAsync();
            CreateAssessmentRequest request = new CreateAssessmentRequest
            {
                ParentAsProjectName = ProjectName.FromProject(projectId),
                Assessment = new Assessment { Event = assessmentEvent },
            };
            return await client.CreateAssessmentAsync(request);
        }
    }
}
