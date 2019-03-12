using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Does type implement the inherited type?
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="inherited">The inheritable type we are checking for an implementation of.</param>
        /// <returns></returns>
        public static bool Implements(this Type type, Type inherited)
        {
            try
            {
                var genericType = inherited.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    return genericType.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

    }

}
