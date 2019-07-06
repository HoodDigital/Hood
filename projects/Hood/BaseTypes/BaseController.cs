using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
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
        protected readonly IMailService _mailService;
        protected readonly ISmsSender _smsSender;
        protected readonly IBillingService _billing;
        protected readonly ILogService _logService;
        protected readonly IConfiguration _config;
        protected readonly IHostingEnvironment _env;
        protected readonly IHoodCache _cache;
        protected readonly IMediaManager<MediaObject> _media;
        protected readonly IAddressService _address;
        protected readonly IEventsService _eventService;

        [TempData]
        public string SaveMessage { get; set; }
        [TempData]
        public AlertType MessageType { get; set; }

        public BaseController()
        {
            _userManager = Engine.Current.Resolve<UserManager<ApplicationUser>>();
            _signInManager = Engine.Current.Resolve<SignInManager<ApplicationUser>>();
            _roleManager = Engine.Current.Resolve<RoleManager<IdentityRole>>();
            _content = Engine.Current.Resolve<IContentRepository>();
            _contentCategoryCache = Engine.Current.Resolve<ContentCategoryCache>();
            _property = Engine.Current.Resolve<IPropertyRepository>();
            _forumCategoryCache = Engine.Current.Resolve<ForumCategoryCache>();
            _account = Engine.Current.Resolve<IAccountRepository>();
            _db = Engine.Current.Resolve<TContext>();
            _emailSender = Engine.Current.Resolve<IEmailSender>();
            _mailService = Engine.Current.Resolve<IMailService>();
            _smsSender = Engine.Current.Resolve<ISmsSender>();
            _billing = Engine.Current.Resolve<IBillingService>();
            _logService = Engine.Current.Resolve<ILogService>();
            _config = Engine.Current.Resolve<IConfiguration>();
            _env = Engine.Current.Resolve<IHostingEnvironment>();
            _cache = Engine.Current.Resolve<IHoodCache>();
            _address = Engine.Current.Resolve<IAddressService>();
            _eventService = Engine.Current.Resolve<IEventsService>();
            _media = Engine.Current.Resolve<IMediaManager<MediaObject>>();
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

        public UserSubscriptionsView Account
        {
            get => User.AccountInfo();
        }
    }
}