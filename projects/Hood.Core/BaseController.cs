using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public abstract class BaseController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public BaseController()
            :base()
        {

        }
    }
    public abstract class BaseController<TContext, TUser, TRole> : Controller
         where TContext : HoodDbContext
         where TUser : ApplicationUser
         where TRole : IdentityRole
    {
        protected readonly UserManager<TUser> _userManager;
        protected readonly SignInManager<TUser> _signInManager;
        protected readonly RoleManager<TRole> _roleManager;
        protected readonly IAccountRepository _account;
        protected readonly IContentRepository _content;
        protected readonly ContentCategoryCache _contentCategoryCache;
        protected readonly IPropertyRepository _property;
        protected readonly TContext _db;
        protected readonly IEmailSender _emailSender;
        protected readonly IMailService _mailService;
        protected readonly ISmsSender _smsSender;
        protected readonly ILogService _logService;
        protected readonly IConfiguration _config;
        protected readonly IWebHostEnvironment _env;
        protected readonly IHoodCache _cache;
        protected readonly IDirectoryManager _directoryManager;
        protected readonly IMediaManager _media;
        protected readonly IAddressService _address;
        protected readonly IThemesService _themeService;
        protected readonly IEventsService _eventService;
        protected readonly IRecaptchaService _recaptcha;

        [TempData]
        public string SaveMessage { get; set; }
        [TempData]
        public AlertType MessageType { get; set; }

        public BaseController()
        {
            _userManager = Engine.Services.Resolve<UserManager<TUser>>();
            _signInManager = Engine.Services.Resolve<SignInManager<TUser>>();
            _roleManager = Engine.Services.Resolve<RoleManager<TRole>>();
            _content = Engine.Services.Resolve<IContentRepository>();
            _contentCategoryCache = Engine.Services.Resolve<ContentCategoryCache>();
            _property = Engine.Services.Resolve<IPropertyRepository>();
            _account = Engine.Services.Resolve<IAccountRepository>();
            _db = Engine.Services.Resolve<TContext>();
            _emailSender = Engine.Services.Resolve<IEmailSender>();
            _mailService = Engine.Services.Resolve<IMailService>();
            _smsSender = Engine.Services.Resolve<ISmsSender>();
            _logService = Engine.Services.Resolve<ILogService>();
            _config = Engine.Services.Resolve<IConfiguration>();
            _env = Engine.Services.Resolve<IWebHostEnvironment>();
            _cache = Engine.Services.Resolve<IHoodCache>();
            _address = Engine.Services.Resolve<IAddressService>();
            _eventService = Engine.Services.Resolve<IEventsService>();
            _directoryManager = Engine.Services.Resolve<IDirectoryManager>();
            _themeService = Engine.Services.Resolve<IThemesService>();
            _media = Engine.Services.Resolve<IMediaManager>();
            _recaptcha = Engine.Services.Resolve<IRecaptchaService>();
        }

        public ViewResult View(ISaveableModel model)
        {
            model.MessageType = MessageType;
            model.SaveMessage = SaveMessage;
            return base.View(model);
        }
        public ViewResult View(string viewName, ISaveableModel model)
        {
            model.MessageType = MessageType;
            model.SaveMessage = SaveMessage;
            return base.View(viewName, model);
        }

        public async Task<Response> SuccessResponseAsync<TSource>(string successMessage, string title = null)
        {
            await _logService.AddLogAsync<TSource>(successMessage, type: LogType.Success);
            return new Response(true, successMessage, title);
        }
        public async Task<Response> SuccessResponseAsync<TSource>(string successMessage, object logObject, string title = null)
        {
            await _logService.AddLogAsync<TSource>(successMessage, logObject, type: LogType.Success);
            return new Response(true, successMessage, title);
        }
        public async Task<Response> ErrorResponseAsync<TSource>(string errorMessage, Exception ex)
        {
            await _logService.AddExceptionAsync<TSource>(errorMessage, ex);
            return new Response(ex, errorMessage);
        }
        public async Task<Response> ErrorResponseAsync<TSource>(string errorMessage, Exception ex, object logObject)
        {
            await _logService.AddExceptionAsync<TSource>(errorMessage, logObject, ex);
            return new Response(ex, errorMessage);
        }

    }
}