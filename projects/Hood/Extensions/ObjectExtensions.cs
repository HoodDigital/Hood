using Hood.Entities;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Hood.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Will return a string representation of the object and all it's child members. In JSON format.
        /// </summary>
        /// <param name="element">The object or class you want to print to JSON.</param>
        /// <param name="indentSize">The number of tab characters inserted onto each sub line.</param>
        /// <returns>String Json content</returns>
        public static string ToJson(this object element)
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
        /// Will return a string representation of the object and all it's child members. In JSON format.
        /// </summary>
        /// <param name="element">The object or class you want to print to JSON.</param>
        /// <param name="indentSize">The number of tab characters inserted onto each sub line.</param>
        /// <returns>String Json content</returns>
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
        public static void CopyProperties(this object source, object destination, bool updateOnly = false)
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
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
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
    }
}
