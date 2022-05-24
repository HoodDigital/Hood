using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IRecaptchaService
    {
        System.Threading.Tasks.Task<RecaptchaResponse> Validate(HttpRequest request);
    }
}