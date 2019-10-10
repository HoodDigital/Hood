using Hood.Entities;
using Hood.Extensions;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace Hood.Models
{
    public abstract class MetadataBase : BaseEntity, IMetadata
    {
        public MetadataBase()
        {
        }
        public MetadataBase(string name, string value, string type = "System.String")
        {
            Type = type;
            Name = name;
            BaseValue = value;
        }

        public string BaseValue { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public string InputId => $"Meta-{Name.ToSeoUrl()}";
        public string InputName => $"Meta:{Name}";
        public string InputDisplayName
        {
            get
            {
                if (IsTemplate)
                {
                    var name = Name.Split('.')[Name.Split('.').Length - 2].Replace("-", " ").CamelCaseToString().ToTitleCase();
                    return $"{name} - {InputType}";
                }

                return Name.Split('.').Last().CamelCaseToString().ToTitleCase();
            }
        }
        public string InputType
        {
            get
            {
                if (IsTemplate)
                {
                    return Name.Split('.')[Name.Split('.').Length - 1];
                }

                return Type;
            }
        }
        public new System.Type GetType
        {
            get
            {
                Assembly a = Assembly.Load("Hood");
                return a.GetType(Type);
            }
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

        public bool IsTemplate => Name.StartsWith("Template.");
        public bool IsImageSetting => Name.StartsWith("Settings.Image.");
    }
}
