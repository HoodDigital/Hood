using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hood.Core
{
    public interface ITypeFinder
    {
        /// <summary>
        /// Find all classes of type from all loaded assemblies.
        /// </summary>
        /// <param name="onlyConcreteClasses">Search for only concrete classes, set as false to include abstracts or interfaces.</param>
        /// <typeparam name="T">The type to check for.</typeparam>
        /// <returns>List of Types that implement the supplied type.</returns>
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

        /// <summary>
        /// Gets the assemblies currently loaded into the AppDomain.
        /// </summary>
        /// <returns>A complete list of loaded assemblies.</returns>
        IList<Assembly> GetAssemblies();
    }
}
