using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hood.Extensions
{
    public static class ExceptionExtensions
    {
        public static Dictionary<string, string> ToDictionary(this Exception ex)
        {
            var error = new Dictionary<string, string>
                {
                    {"Type", ex.GetType().ToString()},
                    {"Message", ex.Message},
                    {"StackTrace", ex.StackTrace}
                };

            foreach (DictionaryEntry data in ex.Data)
                error.Add(data.Key.ToString(), data.Value.ToString());

            if (ex.InnerException != null)
            {
                var innerError = new Dictionary<string, string>
                    {
                        {"Type", ex.InnerException.GetType().ToString()},
                        {"Message", ex.InnerException.Message},
                        {"StackTrace", ex.InnerException.StackTrace}
                    };

                foreach (DictionaryEntry data in ex.InnerException.Data)
                    error.Add(data.Key.ToString(), data.Value.ToString());

                string innerJson = JsonConvert.SerializeObject(innerError, Formatting.Indented);
                error.Add("InnerException", innerJson);
            }

            return error;
        }
    }
}
