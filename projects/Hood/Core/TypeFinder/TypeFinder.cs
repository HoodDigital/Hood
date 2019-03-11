using Hood.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hood.Core
{
    public class TypeFinder : ITypeFinder
    {
        public TypeFinder()
        { }

        #region Methods

        /// <summary>
        /// Find all classes of type from all loaded assemblies.
        /// </summary>
        /// <param name="onlyConcreteClasses">Search for only concrete classes, set as false to include abstracts or interfaces.</param>
        /// <typeparam name="T">The type to check for.</typeparam>
        /// <returns>List of Types that implement the supplied type.</returns>
        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            var assemblies = GetAssemblies();
            var assignTypeFrom = typeof(T);
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch
                    {
                    }

                    if (types == null)
                        continue;

                    foreach (var t in types)
                    {
                        if (!assignTypeFrom.IsAssignableFrom(t) && (!assignTypeFrom.IsGenericTypeDefinition || !t.Implements(assignTypeFrom)))
                            continue;

                        if (t.IsInterface)
                            continue;

                        if (onlyConcreteClasses)
                        {
                            if (t.IsClass && !t.IsAbstract)
                            {
                                result.Add(t);
                            }
                        }
                        else
                        {
                            result.Add(t);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }

            return result;
        }

        /// <summary>
        /// Gets the assemblies currently loaded into the AppDomain.
        /// </summary>
        /// <returns>A complete list of loaded assemblies.</returns>
        public virtual IList<Assembly> GetAssemblies()
        {
            var added = new List<string>();
            var assemblies = new List<Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Regex.IsMatch(assembly.FullName, IngoreAssemblies, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                    continue;

                if (added.Contains(assembly.FullName))
                    continue;

                assemblies.Add(assembly);
                added.Add(assembly.FullName);
            }
            return assemblies;
        }

        #endregion

        #region Properties

        /// <summary>Gets the pattern for dlls that we know don't need to be investigated.</summary>
        public string IngoreAssemblies { get; set; } = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

        #endregion
    }
}