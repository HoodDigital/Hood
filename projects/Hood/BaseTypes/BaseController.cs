using Hood.Caching;
using Hood.Core;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Controllers
{
    public abstract class BaseController<TContext, TUser, TRole> : Controller
         where TContext : HoodDbContext
         where TUser : ApplicationUser
         where TRole : IdentityRole
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly IAccountRepository _account;
        protected readonly IContentRepository _content;
        protected readonly ContentCategoryCache _contentCategoryCache;
        protected readonly IPropertyRepository _property;
        protected readonly ForumCategoryCache _forumCategoryCache;
        protected readonly TContext _db;
        protected readonly IEmailSender _emailSender;
        protected readonly ISmsSender _smsSender;
        protected readonly ISettingsRepository _settings;
        protected readonly IBillingService _billing;
        protected readonly ILogService _logService;
        protected readonly IConfiguration _config;
        protected readonly IHostingEnvironment _env;
        protected readonly IHoodCache _cache;
        protected readonly IMediaManager<MediaObject> _media;
        protected readonly IAddressService _address;
        protected readonly IEventsService _eventService;

        public BaseController()
        {
            _userManager = EngineContext.Current.Resolve<UserManager<ApplicationUser>>();
            _signInManager = EngineContext.Current.Resolve<SignInManager<ApplicationUser>>();
            _roleManager = EngineContext.Current.Resolve<RoleManager<IdentityRole>>();
            _content = EngineContext.Current.Resolve<IContentRepository>();
            _contentCategoryCache = EngineContext.Current.Resolve<ContentCategoryCache>();
            _property = EngineContext.Current.Resolve<IPropertyRepository>();
            _forumCategoryCache = EngineContext.Current.Resolve<ForumCategoryCache>();
            _account = EngineContext.Current.Resolve<IAccountRepository>();
            _db = EngineContext.Current.Resolve<TContext>();
            _emailSender = EngineContext.Current.Resolve<IEmailSender>();
            _smsSender = EngineContext.Current.Resolve<ISmsSender>();
            _settings = EngineContext.Current.Resolve<ISettingsRepository>();
            _billing = EngineContext.Current.Resolve<IBillingService>();
            _logService = EngineContext.Current.Resolve<ILogService>();
            _config = EngineContext.Current.Resolve<IConfiguration>();
            _env = EngineContext.Current.Resolve<IHostingEnvironment>();
            _cache = EngineContext.Current.Resolve<IHoodCache>();
            _address = EngineContext.Current.Resolve<IAddressService>();
            _eventService = EngineContext.Current.Resolve<IEventsService>();
            _media = EngineContext.Current.Resolve<IMediaManager<MediaObject>>();
        }
    }
}
