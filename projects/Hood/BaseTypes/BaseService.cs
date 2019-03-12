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
            _userManager = Engine.Current.Resolve<UserManager<ApplicationUser>>();
            _signInManager = Engine.Current.Resolve<SignInManager<ApplicationUser>>();
            _roleManager = Engine.Current.Resolve<RoleManager<IdentityRole>>();
            _db = Engine.Current.Resolve<TContext>();
            _email = Engine.Current.Resolve<IEmailSender>();
            _settings = Engine.Current.Resolve<ISettingsRepository>();
            _billing = Engine.Current.Resolve<IBillingService>();
            _logService = Engine.Current.Resolve<ILogService>();
            _config = Engine.Current.Resolve<IConfiguration>();
            _env = Engine.Current.Resolve<IHostingEnvironment>();
            _cache = Engine.Current.Resolve<IHoodCache>();
            _address = Engine.Current.Resolve<IAddressService>();
            _eventService = Engine.Current.Resolve<IEventsService>();
        }
    }
}
