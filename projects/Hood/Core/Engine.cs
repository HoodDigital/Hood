using Hood.Caching;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
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
                var _contextAccessor = Services.Resolve<IHttpContextAccessor>();
                return _contextAccessor.HttpContext.User.AccountInfo();
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

        public static string Version
        {
            get
            {
                var version = typeof(Engine).Assembly.GetName().Version;
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        #endregion
    }
}
