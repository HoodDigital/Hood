using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hood.Attributes;
using Hood.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hood.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="allowedKeys">The form keys that are allowed to be copied.</param>
        public static T UpdateFromFormModel<T, TSource>(
            this T destination,
            TSource source,
            HashSet<string> allowedKeys
        )
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and
            // populate them from their desination counterparts
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (
                    targetProperty.GetSetMethod(true) != null
                    && targetProperty.GetSetMethod(true).IsPrivate
                )
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }
                if (!Attribute.IsDefined(targetProperty, typeof(FormUpdatableAttribute)))
                {
                    continue;
                }
                if (!allowedKeys.Contains(srcProp.Name))
                {
                    continue;
                }

                // Passed all tests, lets set the value
                object newValue = srcProp.GetValue(source, null);
                if (newValue == null && srcProp.PropertyType.FullName == "System.String")
                    newValue = "";
                targetProperty.SetValue(destination, newValue, null);
            }

            return destination;
        }

        /// <summary>
        /// Will return a string representation of the object and all it's child members. In JSON format.
        /// </summary>
        /// <param name="element">The object or class you want to print to JSON.</param>
        /// <returns>String Json content</returns>
        public static string ToJson(this object element)
        {
            try
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                };
                return JsonConvert.SerializeObject(
                    element,
                    new JsonSerializerSettings
                    {
                        ContractResolver = contractResolver,
                        Formatting = Formatting.None,
                    }
                );
            }
            catch (Exception ex)
            {
                return ex.ToJson();
            }
        }

        /// <summary>
        /// Returns a cache key for the entity — its JSON-serialised representation.
        /// </summary>
        /// <param name="element">The entity to generate a cache key for.</param>
        /// <returns>String cache key</returns>
        public static string GetCacheKey(this BaseEntity element)
        {
            try
            {
                return JsonConvert.SerializeObject(element);
            }
            catch (Exception ex)
            {
                return ex.ToJson();
            }
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="updateOnly">When true, only set properties whose value has changed.</param>
        public static void CopyProperties(
            this object source,
            object destination,
            bool updateOnly = false
        )
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and
            // populate them from their desination counterparts
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (
                    targetProperty.GetSetMethod(true) != null
                    && targetProperty.GetSetMethod(true).IsPrivate
                )
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }
                // Passed all tests, lets set the value
                object currentValue = targetProperty.GetValue(destination, null);
                object newValue = srcProp.GetValue(source, null);
                if (newValue == null && srcProp.PropertyType.FullName == "System.String")
                    newValue = "";
                if (updateOnly)
                {
                    if (newValue != currentValue)
                        targetProperty.SetValue(destination, newValue, null);
                }
                else
                {
                    targetProperty.SetValue(destination, newValue, null);
                }
            }
        }

        public static RouteValueDictionary ToRouteValueDictionary(this object obj)
        {
            var routeParms = obj.GetType().GetProperties();
            var routeValues = new RouteValueDictionary();
            routeParms.ForEach(p =>
            {
                if (!p.GetCustomAttributes(typeof(RouteIgnoreAttribute), false).Any())
                {
                    var name = p.Name;
                    var queryAttr =
                        p.GetCustomAttributes(typeof(FromQueryAttribute), false).FirstOrDefault()
                        as FromQueryAttribute;
                    if (queryAttr != null)
                    {
                        name = queryAttr.Name;
                    }
                    var routeAttr =
                        p.GetCustomAttributes(typeof(FromRouteAttribute), false).FirstOrDefault()
                        as FromRouteAttribute;
                    if (routeAttr != null)
                    {
                        name = routeAttr.Name;
                    }
                    var value = p.GetValue(obj, null);
                    if (value != null)
                    {
                        routeValues.Add(name, value);
                    }
                }
            });
            return routeValues;
        }
    }
}
