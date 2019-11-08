using Hood.Extensions;
using Newtonsoft.Json;
using System;

namespace Hood.Models
{
    public interface IMetadata
    {
        string BaseValue { get; }
        int Id { get; set; }
        string InputDisplayName { get; }
        string InputId { get; }
        string InputName { get; }
        string InputType { get; }
        bool IsImageSetting { get; }
        bool IsTemplate { get; }
        string Name { get; set; }
        string Type { get; set; }

        void SetValue(string value);
        T GetValue<T>();
        string ToString();
    }

    public static class IMetadataExtensions
    {
        public static T Get<T>(this IMetadata meta)
        {
            if (meta.BaseValue.IsSet())
            {
                return JsonConvert.DeserializeObject<T>(meta.BaseValue);
            }
            else
            {
                return default(T);
            }
        }

        public static string GetStringValue(this IMetadata meta)
        {
            try
            {
                string ret = JsonConvert.DeserializeObject<string>(meta.BaseValue);
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