using Hood.Caching;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hood.Core
{
    /// <summary>
    /// Globally accessible class containing static accessors to key areas of the Hood CMS Engine.
    /// </summary>
    public class Engine
    {
        #region Methods

        /// <summary>
        /// Create a static instance of the Hood engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IHoodServiceProvider CreateHoodServiceProvider()
        {
            if (Singleton<IHoodServiceProvider>.Instance == null)
                Singleton<IHoodServiceProvider>.Instance = new HoodServiceProvider();

            return Singleton<IHoodServiceProvider>.Instance;
        }

        #endregion

        #region Static Accessors

        /// <summary>
        /// Gets the singleton HoodServiceProvider used to manage and provide global access to Hood services.
        /// </summary>
        public static IHoodServiceProvider Services
        {
            get
            {
                if (Singleton<IHoodServiceProvider>.Instance == null)
                {
                    CreateHoodServiceProvider();
                }

                return Singleton<IHoodServiceProvider>.Instance;
            }
        }

        public static Assembly ResolveUI(string uiName)
        {
            // Register all Hood Components
            var typeFinder = Engine.Services.Resolve<ITypeFinder>();
            var dependencies = typeFinder.FindClassesOfType<IHoodComponent>();

            var instances = dependencies
                                .Select(dependencyRegistrar => (IHoodComponent)Activator.CreateInstance(dependencyRegistrar))
                                .OrderBy(dependencyRegistrar => dependencyRegistrar.ServiceConfigurationOrder);

            foreach (var dependency in instances)
            {
                if (dependency.IsUIComponent && dependency.Name == uiName)
                    return dependency.GetType().Assembly;
            }

            return null;
        }

        public static HoodConfiguration Configuration
        {
            get
            {
                var config = Services.Resolve<IOptions<HoodConfiguration>>();
                if (config != null)
                {
                    return config.Value;
                }
                else
                    return null;
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the ISettingsRepository.
        /// </summary>
        public static ISettingsRepository Settings
        {
            get
            {
                return Services.Resolve<ISettingsRepository>();
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the IHoodCache.
        /// </summary>
        public static IHoodCache Cache
        {
            get
            {
                return Services.Resolve<IHoodCache>();
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the ILogService.
        /// </summary>
        public static ILogService Logs
        {
            get
            {
                return Services.Resolve<ILogService>();
            }
        }
        /// <summary>
        /// Gets the current user's account, from context, cache or datastore.
        /// </summary>
        public static UserProfile Account
        {
            get
            {
                try
                {
                    var _contextAccessor = Services.Resolve<IHttpContextAccessor>();
                    if (_contextAccessor == null ||
                        _contextAccessor.HttpContext == null ||
                        _contextAccessor.HttpContext.User == null ||
                        !_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
                        return null;
                    return _contextAccessor.HttpContext.User.GetUserProfile();
                }
                catch (SqlException)
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the IAccountRepository.
        /// </summary>
        public static IAccountRepository AccountManager
        {
            get
            {
                return Services.Resolve<IAccountRepository>();
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the IMediaManager<MediaObject>.
        /// </summary>
        public static IMediaManager Media
        {
            get
            {
                return Services.Resolve<IMediaManager>();
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the IAccountRepository.
        /// </summary>
        public static IThemesService Themes
        {
            get
            {
                return Services.Resolve<IThemesService>();
            }
        }
        /// <summary>
        /// Gets the current resolvable version of the IEventsService.
        /// </summary>
        public static IEventsService Events
        {
            get
            {
                return Services.Resolve<IEventsService>();
            }
        }

        public static string Resource(string localPath)
        {
            var env = Engine.Services.Resolve<IWebHostEnvironment>();
            switch (env.EnvironmentName)
            {
                case "Hood":
                    return localPath;
                default:
                    if (Configuration.BypassCDN)
                    {
                        return localPath;
                    }
                    return $"https://cdn.jsdelivr.net/npm/hoodcms@{Version}{localPath}";
            }
        }

        /// <summary>
        /// <para>Gets the location for the Hood Client Side library folder containing all CSS/JS for the app.</para>
        /// <para>Default is "/hood/".</para>
        /// <para>Should always start with '/' and end with '/'.</para>
        /// <para>Config section Hood:LibraryFolder</para>
        /// </summary>
        public static string SiteOwnerEmail
        {
            get
            {
                if (Configuration != null)
                {
                    return Configuration.SuperAdminEmail;
                }
                return "admin@hooddigital.com";
            }
        }

        public static string Version
        {
            get
            {
                var version = typeof(Engine).Assembly.GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        public static string Url
        {
            get
            {
                if (Settings["Hood.SiteUrl"] != null)
                {
                    return Settings["Hood.SiteUrl"].ToString();
                }
                return null;
            }
        }

        public static string ApplicationKey
        {
            get
            {
                if (Settings["Hood.Api.SystemPrivateKey"] != null)
                {
                    return Settings["Hood.Api.SystemPrivateKey"].ToString();
                }
                return null;
            }
        }

        #endregion
    }
}
