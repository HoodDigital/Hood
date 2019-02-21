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
        public static IEngine Create()
        {
            //create NopEngine as engine
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
                    Create();
                }

                return Singleton<IEngine>.Instance;
            }
        }

        #endregion
    }
}
