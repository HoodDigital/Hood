using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;

namespace Hood.Api.BaseControllers
{
    [ApiController]
    public abstract class HomeController : ControllerBase
    {
        public HomeController() : base()
        { }

        private const string publicMessage = "The API doesn't require an access token to share this message.";
        private const string protectedMessage = "The API successfully validated your access token.";
        private const string adminMessage = "The API successfully recognized you as an admin.";

        [HttpGet("")]
        public string Index()
        {
            return $"Hood API.";
        }

        [HttpGet("public")]
        public ApiResponse GetPublicMessage()
        {
            return new ApiResponse(publicMessage);
        }

        [HttpGet("protected")]
        [Authorize]
        public ApiResponse GetProtectedMessage()
        {
            return new ApiResponse(protectedMessage);
        }

        [HttpGet("admin")]
        [Authorize(Policy = "Admin")]
        public ApiResponse GetAdminMessage()
        {
            return new ApiResponse(adminMessage);
        }

    }
}
