using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models
{
    class MediaObjectJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType().IsAssignableFrom(typeof(IMediaObject)))
            {
                var info = value as IMediaObject;
                serializer.Serialize(writer, new { info.Url, Thumnbnail = info.SmallUrl });
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
