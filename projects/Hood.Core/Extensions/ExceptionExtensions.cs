using Hood.Core;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
    [Serializable]
    public class StartupException : Exception
    {
        public StartupError Error { get; set; }

        public StartupException(string message, StartupError error) : base(message)
        {
            ProcessError(error);
        }

        public StartupException(string message, Exception innerException, StartupError error) : base(message, innerException)
        {
            ProcessError(error);
        }

        protected StartupException(SerializationInfo info, StreamingContext context, StartupError error) : base(info, context)
        {
            ProcessError(error);
        }

        private void ProcessError(StartupError error)
        {
            Error = error;
            switch (Error)
            {
                case StartupError.MigrationMissing:
                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.DatabaseMigrationsMissing = true;
                    break;
                case StartupError.MigrationNotApplied:
                    Engine.Services.DatabaseMigrationsMissing = false;
                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.MigrationNotApplied = true;
                    break;
                case StartupError.AdminUserSetupError:
                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.DatabaseMigrationsMissing = false;
                    Engine.Services.MigrationNotApplied = false;
                    Engine.Services.AdminUserSetupError = true;
                    break;
                case StartupError.DatabaseConnectionFailed:
                    Engine.Services.DatabaseConnectionFailed = true;
                    Engine.Services.DatabaseMigrationsMissing = true;
                    Engine.Services.MigrationNotApplied = true;
                    Engine.Services.AdminUserSetupError = true;
                    break;
                case StartupError.DatabaseMediaError:
                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.DatabaseMigrationsMissing = false;
                    Engine.Services.MigrationNotApplied = false;
                    Engine.Services.AdminUserSetupError = false;
                    Engine.Services.DatabaseMediaTimeout = true;
                    break;
                case StartupError.DatabaseViewsNotInstalled: 
                    Engine.Services.ViewsInstalled = false;
                    break;
            }
        }
    }
    public enum StartupError
    {
        MigrationMissing,
        MigrationNotApplied,
        DatabaseConnectionFailed,
        AdminUserSetupError,
        DatabaseMediaError,
        DatabaseViewsNotInstalled,
        Auth0Issue
    }
}
