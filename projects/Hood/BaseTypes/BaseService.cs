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

namespace Hood.Services
{
    public abstract class BaseService<TContext, TUser, TRole>
         where TContext : HoodDbContext
         where TUser : ApplicationUser
         where TRole : IdentityRole
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly TContext _db;
        protected readonly IEmailSender _email;
        protected readonly ISettingsRepository _settings;
        protected readonly IBillingService _billing;
        protected readonly ILogService _logService;
        protected readonly IConfiguration _config;
        protected readonly IHostingEnvironment _env;
        protected readonly IHoodCache _cache;
        protected readonly IMediaManager<MediaObject> _media;
        protected readonly IAddressService _address;
        protected readonly IEventsService _eventService;

        public BaseService()
        {
            _userManager = EngineContext.Current.Resolve<UserManager<ApplicationUser>>();
            _signInManager = EngineContext.Current.Resolve<SignInManager<ApplicationUser>>();
            _roleManager = EngineContext.Current.Resolve<RoleManager<IdentityRole>>();
            _db = EngineContext.Current.Resolve<TContext>();
            _email = EngineContext.Current.Resolve<IEmailSender>();
            _settings = EngineContext.Current.Resolve<ISettingsRepository>();
            _billing = EngineContext.Current.Resolve<IBillingService>();
            _logService = EngineContext.Current.Resolve<ILogService>();
            _config = EngineContext.Current.Resolve<IConfiguration>();
            _env = EngineContext.Current.Resolve<IHostingEnvironment>();
            _cache = EngineContext.Current.Resolve<IHoodCache>();
            _address = EngineContext.Current.Resolve<IAddressService>();
            _eventService = EngineContext.Current.Resolve<IEventsService>();
        }
    }
}
