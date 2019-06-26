using Hood.BaseTypes;
using System;
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
        public List<ContentType> AllowedTypes
        {
            get => Types.Where(t => t.Enabled).ToList();
        }
        public List<ContentType> DisallowedTypes
        {
            get => Types.Where(t => !t.Enabled).ToList();
        }
        public List<ContentType> PublicTypes
        {
            get => Types.Where(t => t.IsPublic && t.Enabled).ToList();
        }
        public List<ContentType> RestrictedTypes
        {
            get => Types.Where(t => !t.IsPublic || !t.Enabled).ToList();
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

        #region Obsolete
        [Obsolete("Use AllowedTypes instead.", true)]
        public List<ContentType> GetAllowedTypes() => throw new NotImplementedException();
        [Obsolete("Use DisallowedTypes instead.", true)]
        public List<ContentType> GetDisallowedTypes() => throw new NotImplementedException();
        [Obsolete("Use PublicTypes instead.", true)]
        public List<ContentType> GetPublicTypes() => throw new NotImplementedException();
        [Obsolete("Use RestrictedTypes instead.", true)]
        public List<ContentType> GetRestrictedTypes() => throw new NotImplementedException();
        #endregion
    }
}