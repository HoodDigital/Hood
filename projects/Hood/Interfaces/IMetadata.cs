using Hood.Extensions;
using Newtonsoft.Json;

namespace Hood.Interfaces
{
    public interface IMetadata
    {
        int Id { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        string BaseValue { get; set; }
    }

    public static class IMetadataExtensions
    {
        public static T Get<T>(this IMetadata meta)
        {
            if (meta.BaseValue.IsSet())
                return JsonConvert.DeserializeObject<T>(meta.BaseValue);
            else
                return default(T);
        }

        public static void Set<T>(this IMetadata meta, T value)
        {
            string val = JsonConvert.SerializeObject(value);
            meta.BaseValue = val;
            if (!meta.Type.IsSet())
                meta.Type = value.GetType().ToString();
        }

        public static string GetStringValue(this IMetadata meta)
        {
            try
            {
                var ret = JsonConvert.DeserializeObject<string>(meta.BaseValue);
                return ret.IsSet() ? ret : "";
            }
            catch
            {
                if (!meta.BaseValue.IsSet())
                {
                    return "";
                }
                else
                {
                    return JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(meta.BaseValue));
                }
            }
        }

    }
}

