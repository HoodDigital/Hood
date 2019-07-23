using System;
using System.Runtime.Serialization;

namespace Hood.Services
{
    [Serializable]
    internal class AlertedException : Exception
    {
        public AlertedException()
        {
        }

        public AlertedException(string message) : base(message)
        {
        }

        public AlertedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AlertedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}