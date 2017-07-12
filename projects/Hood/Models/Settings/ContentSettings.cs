using Hood.BaseTypes;
using System.Collections.Generic;
using System.Linq;
using System;

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

        internal void CheckBaseFields()
        {
            foreach (var systemType in ContentTypes.All)
            {
                // Check that this field type exists still.
                for (int i = 0; i < Types.Count(); i++)
                {
                    if (Types[i].BaseName == systemType.BaseName)
                    {
                        foreach (var systemField in systemType.CustomFields)
                        {
                            if (Types[i].CustomFields.Find(f => f.Name == systemField.Name) == null)
                            {
                                var fieldsTemp = Types[i].CustomFields;
                                fieldsTemp.Add(new CustomField()
                                {
                                    System = systemField.System,
                                    Default = systemField.Default,
                                    Name = systemField.Name,
                                    Type = systemField.Type
                                });
                                Types[i].CustomFields = fieldsTemp;
                            }
                            else
                            {
                                Types[i].CustomFields.Find(f => f.Name == systemField.Name).Type = systemField.Type;
                                Types[i].CustomFields.Find(f => f.Name == systemField.Name).Default = systemField.Default;
                                Types[i].CustomFields.Find(f => f.Name == systemField.Name).System = systemField.System;
                            }
                        }
                    }
                }
            }
        }
    }
}