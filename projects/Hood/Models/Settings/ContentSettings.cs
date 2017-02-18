using Hood.BaseTypes;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class ContentSettings : SaveableModel
    {
        public ContentType[] Types { get; set; }

        public ContentSettings()
        {
            Types = ContentTypes.All.ToArray();
        }

        public ContentType GetContentType(string slug)
        {
            var type = Types.Where(t => t.Slug == slug || t.Type == slug || t.TypeNamePlural.ToLower() == slug).FirstOrDefault();
            if (type != null)
                return type;
            return null;
        }
        public List<ContentType> GetAllowedTypes()
        {
            return Types.Where(t => t.Enabled).ToList();
        }
        public List<ContentType> GetDisallowedTypes()
        {
            return Types.Where(t => !t.Enabled).ToList();
        }
        public List<ContentType> GetPublicTypes()
        {
            return Types.Where(t => t.IsPublic && t.Enabled).ToList();
        }
        public List<ContentType> GetRestrictedTypes()
        {
            return Types.Where(t => !t.IsPublic || !t.Enabled).ToList();
        }

    }
}