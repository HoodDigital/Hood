using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models
{
    class ApplicationUserJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(ApplicationUser))
            {
                var info = new UserInfo(value as ApplicationUser);
                serializer.Serialize(writer, info);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ApplicationUser);
        }
    }
}
