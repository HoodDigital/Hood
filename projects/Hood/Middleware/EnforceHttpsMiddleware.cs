using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Hood.Middleware
{
    public class EnforceHttpsMiddleware
    {
        private readonly RequestDelegate _next;

        public EnforceHttpsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            HttpRequest req = context.Request;
            if (req.IsHttps == false)
            {
                string url = "https://" + req.Host + req.Path + req.QueryString;
                context.Response.Redirect(url, permanent: true);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
