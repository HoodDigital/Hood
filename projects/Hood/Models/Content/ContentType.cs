using Hood.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class ContentType
    {
        public string BaseName { get; set; }
        public bool Enabled { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string Description { get; set; }
        public string Search { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string TypeNamePlural { get; set; }
        public string Icon { get; set; }
        public bool HasPage { get; set; }
        public bool IsPublic { get; set; }
        public string TitleName { get; set; }
        public string ExcerptName { get; set; }
        public bool ShowPreview { get; set; }
        public bool ShowEditor { get; set; }
        public bool ShowDesigner { get; set; }
        public bool MultiLineExcerpt { get; set; }
        public bool RichTextExcerpt { get; set; }
        public bool ShowCategories { get; set; }
        public bool ShowImage { get; set; }
        public bool ShowBanner { get; set; }
        public bool ShowMeta { get; set; }
        public bool Templates { get; set; }
        public bool Gallery { get; set; }
        public string CustomFieldsJson { get; set; }
        public string TemplateFolder { get; set; }
        public string UrlFormatting { get; set; }

        public List<CustomField> CustomFields
        {
            get
            {
                if (!CustomFieldsJson.IsSet())
                    return new List<CustomField>();
                return JsonConvert.DeserializeObject<List<CustomField>>(CustomFieldsJson);
            }
            set
            {
                CustomFieldsJson = JsonConvert.SerializeObject(value);
            }
        }

        public bool CachedByType { get; internal set; }

        public CustomField GetMetaDetails(string name)
        {
            var field = CustomFields.SingleOrDefault(c => c.Name == name);
            if (field != null)
                return field;
            var baseType = ContentTypes.All.SingleOrDefault(t => t.TypeName == TypeName);
            if (baseType != null)
            {
                field = baseType.CustomFields.SingleOrDefault(c => c.Name == name);
                if (field != null)
                    return field;
            }
            return new CustomField()
            {
                Default = "",
                Name = name,
                System = false,
                Type = "System.String"
            };
        }
    }
}

