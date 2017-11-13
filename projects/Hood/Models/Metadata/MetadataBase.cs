using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.Reflection;

namespace Hood.Models
{
    public abstract class MetadataBase : IMetadata
    {
        public int Id { get; set; }
        public string BaseValue { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public new System.Type GetType
        {
            get
            {
                Assembly a = Assembly.Load("Hood");
                return a.GetType(Type);
            }
        }
        public MetadataBase()
        {
        }
        public MetadataBase(string name, string value, string type = "System.String")
        {
            Type = type;
            Name = name;
            BaseValue = value;
        }
        public override string ToString()
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(BaseValue);
            }
            catch
            {
                if (!BaseValue.IsSet())
                {
                    return "";
                }
                else
                {
                    return JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(BaseValue));
                }
            }
        }
        public object GetValue()
        {
            return JsonConvert.DeserializeObject(BaseValue, GetType);
        }
        public T GetValue<T>()
        {
            return JsonConvert.DeserializeObject<T>(BaseValue);
        }
    }
}
