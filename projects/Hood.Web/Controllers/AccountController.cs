using Hood.Models;
using Hood.Services;
using Hood.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hood.Controllers
{
    public class AccountController : BaseAccountController
    {
        public AccountController(
            IContentRepository data, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IEmailSender emailSender, 
            ISmsSender smsSender, 
            IHoodCache cache, 
            ILoggerFactory loggerFactory, 
            IAccountRepository account, 
            ISettingsRepository settings) 
            : base(data, userManager, signInManager, emailSender, smsSender, cache, loggerFactory, account, settings)
        {
        }
    }
}
