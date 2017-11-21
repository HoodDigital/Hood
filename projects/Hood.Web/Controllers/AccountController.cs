using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Hood.ViewModels;
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
