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
            _userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();
            _signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
            _roleManager = Engine.Services.Resolve<RoleManager<IdentityRole>>();
            _db = Engine.Services.Resolve<TContext>();
            _email = Engine.Services.Resolve<IEmailSender>();
            _billing = Engine.Services.Resolve<IBillingService>();
            _logService = Engine.Services.Resolve<ILogService>();
            _config = Engine.Services.Resolve<IConfiguration>();
            _env = Engine.Services.Resolve<IHostingEnvironment>();
            _cache = Engine.Services.Resolve<IHoodCache>();
            _address = Engine.Services.Resolve<IAddressService>();
            _eventService = Engine.Services.Resolve<IEventsService>();
        }
    }
}
