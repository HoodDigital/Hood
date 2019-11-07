using Hood.Entities;
using Hood.Extensions;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hood.Models
{
    public abstract class MetadataBase : BaseEntity, IMetadata
    {
        public MetadataBase()
        {
        }

        public string BaseValue { get; internal set; }
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
                    string name = Name.Split('.')[Name.Split('.').Length - 2].Replace("-", " ").CamelCaseToString().ToTitleCase();
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
        public void SetValue(string value)
        {
            switch (Type)
            {
                case "Hood.Date":
                case "Hood.Time":
                case "System.DateTime":
                    if (DateTime.TryParse(value, out DateTime val))
                    {
                        BaseValue = JsonConvert.SerializeObject(val);
                    }
                    else
                    {
                        BaseValue = JsonConvert.SerializeObject(DateTime.Now);
                    }
                    break;
                case "System.Int32":
                    if (int.TryParse(value, out int intVal))
                    {
                        BaseValue = JsonConvert.SerializeObject(intVal);
                    }
                    else
                    {
                        BaseValue = JsonConvert.SerializeObject(0);
                    }
                    break;
                case "System.Double":
                    if (double.TryParse(value, out double doubleVal))
                    {
                        BaseValue = JsonConvert.SerializeObject(doubleVal);
                    }
                    else
                    {
                        BaseValue = JsonConvert.SerializeObject(0);
                    }
                    break;
                case "System.Decimal":
                    if (decimal.TryParse(value, out decimal decimalVal))
                    {
                        BaseValue = JsonConvert.SerializeObject(decimalVal);
                    }
                    else
                    {
                        BaseValue = JsonConvert.SerializeObject(0);
                    }
                    break;
                case "System.String":
                case "Hood.WYSIWYG":
                case "Hood.ImageUrl":
                case "Hood.MultiLineString":
                default:
                    BaseValue = JsonConvert.SerializeObject(value);
                    break;
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
        public T GetValue<T>()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(BaseValue);
            }
            catch (ArgumentNullException)
            {
                return default;
            }
            catch (JsonSerializationException)
            {
                return default;
            }
        }
        public bool IsTemplate => Name.StartsWith("Template.");
        public bool IsImageSetting => Name.StartsWith("Settings.Image.");
    }
}
