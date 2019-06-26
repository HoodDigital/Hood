using Hood.Services;
using System.Runtime.CompilerServices;

namespace Hood.Core
{
    public class Engine
    {
        #region Methods

        /// <summary>
        /// Create a static instance of the Hood engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine LoadEngine()
        {
            if (Singleton<IEngine>.Instance == null)
                Singleton<IEngine>.Instance = new HoodEngine();

            return Singleton<IEngine>.Instance;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton Hood engine used to access Hood services.
        /// </summary>
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    LoadEngine();
                }

                return Singleton<IEngine>.Instance;
            }
        }
        /// <summary>
        /// Gets the singleton Hood engine used to access Hood services.
        /// </summary>
        public static ISettingsRepository Settings
        {
            get
            {
                return Current.Resolve<ISettingsRepository>();
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
