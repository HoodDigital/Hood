using Hood.Attributes;
using Hood.BaseTypes;
using Hood.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hood.Models
{
    public class ContentType : SaveableModel
    {
        public static ContentType Null
        {
            get
            {
                return new ContentType()
                {
                    BaseName = "Unknown",
                    Slug = "unknown",
                    Type = "unknown",
                    Search = "unknown",
                    Title = "Unknowns",
                    Icon = "fa-newspaper",
                    TypeName = "Unknown",
                    Enabled = false,
                    IsPublic = false,
                    HasPage = false,
                    TypeNamePlural = "Unknown",
                    TitleName = "Title",
                    ExcerptName = "Excerpt",
                    MultiLineExcerpt = true,
                    ShowDesigner = false,
                    ShowEditor = true,
                    ShowCategories = false,
                    ShowBanner = true,
                    ShowImage = true,
                    Gallery = true,
                    UrlFormatting = "news-title",
                    Templates = false,
                    TemplateFolder = "Templates",
                    CustomFields = new List<CustomField>()
                };
            }
        }

        public static ContentType CustomType
        {
            get
            {
                return new ContentType()
                {
                    BaseName = "Custom",
                    Slug = "new-content-type",
                    Type = "Custom",
                    Search = "custom",
                    Title = "Custom Content Items",
                    Icon = "fa-file-o",
                    TypeName = "Custom",
                    Enabled = false,
                    IsPublic = true,
                    HasPage = true,
                    TypeNamePlural = "CustomItems",
                    TitleName = "Title",
                    ExcerptName = "Excerpt",
                    MultiLineExcerpt = true,
                    RichTextExcerpt = true,
                    ShowDesigner = false,
                    ShowEditor = true,
                    ShowCategories = false,
                    ShowBanner = true,
                    ShowImage = true,
                    Gallery = true,
                    UrlFormatting = "news-title",
                    Templates = false,
                    TemplateFolder = "Templates",
                    CustomFields = new List<CustomField>()
                };
            }
        }

        public bool IsUnknown { get { return Type == "unknown"; } }

        [FormUpdatable]
        [Display(Name = "Base Name", Description = "The base type of this content type, sets common features based on the type you select here.")]
        public string BaseName { get; set; }

        [FormUpdatable]
        [Display(Name = "Enabled", Description = "Enable this content type for use on the site.")]
        public bool Enabled { get; set; }

        [FormUpdatable]
        [Display(Name = "Slug", Description = "The base URL slug for accessing this content type etc <code>yourdomain.com/news/</code>")]
        public string Slug { get; set; }

        [FormUpdatable]
        [Display(Name = "Title", Description = "Only use lowercase letters, no spaces.")]
        public string Title { get; set; }

        [FormUpdatable]
        [Display(Name = "Meta Title", Description = "Title field used in <code>meta</code> tags.")]
        public string MetaTitle { get; set; }

        [FormUpdatable]
        [Display(Name = "Meta Description", Description = "Description field used in <code>&lt;meta&gt;</code> tags.")]
        public string Description { get; set; }

        [FormUpdatable]
        [Display(Name = "Search", Description = "")]
        public string Search { get; set; }

        [FormUpdatable]
        [Display(Name = "Type", Description = "This is how your type is stored in the database.")]
        public string Type { get; set; }

        [FormUpdatable]
        [Display(Name = "Type Name", Description = "The singular name of this type, for use in titles etc.")]
        public string TypeName { get; set; }

        [FormUpdatable]
        [Display(Name = "Type Name (Plural)", Description = "The plural name of this type, for use in titles etc.")]
        public string TypeNamePlural { get; set; }

        [FormUpdatable]
        [Display(Name = "Icon", Description = "Choose an icon for this content type (for sidebar link).")]
        public string Icon { get; set; }

        [FormUpdatable]
        [Display(Name = "No Image (Override)", Description = "Override the site default 'No Image' for this content type. Leave it blank to use the site default.")]
        public string NoImage { get; set; }

        [FormUpdatable]
        [Display(Name = "Accessed via Url", Description = "Show individual pages for each record of this type.")]
        public bool HasPage { get; set; }

        [FormUpdatable]
        [Display(Name = "Has Public Page", Description = "Show a page list or grid page showing all types, with search etc. for this type.")]
        public bool IsPublic { get; set; }

        [FormUpdatable]
        [Display(Name = "Title Name", Description = "")]
        public string TitleName { get; set; }

        [FormUpdatable]
        [Display(Name = "Excerpt Name", Description = "")]
        public string ExcerptName { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Preview", Description = "Show the preview panel in the editor, allows you preview the type on various screen sizes while editing.")]
        public bool ShowPreview { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Editor", Description = "Enable a rich text editor on the content body, otherwise no editor will show.")]
        public bool ShowEditor { get; set; }

        [FormUpdatable]
        [Display(Name = "Hide Author", Description = "Hide the author's name, details and avatar from the display page and feeds.")]
        public bool HideAuthor { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Designer", Description = "Enables the inline designer (BETA). This allows the editing of content while viewing it.")]
        public bool ShowDesigner { get; set; }

        [FormUpdatable]
        [Display(Name = "MultiLine Excerpt?", Description = "Set whether or not the excerpt field allows multi-line text.")]
        public bool MultiLineExcerpt { get; set; }

        [FormUpdatable]
        [Display(Name = "Rich Text Excerpt?", Description = "Set whether or not the excerpt field allows rich editable text.")]
        public bool RichTextExcerpt { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Categories", Description = "Allow categories on this type, users can add categories and assign items to them.")]
        public bool ShowCategories { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Image", Description = "Enable the featured image field.")]
        public bool ShowImage { get; set; }

        [FormUpdatable]
        [Display(Name = "Show Share", Description = "Enable the share image field, for Twitter/Facebook social media sharer images.")]
        public bool ShowBanner { get; set; }

        [FormUpdatable]
        [Display(Name = "Enable Templates", Description = "Enable templates for this type, you can select a template to use for the view of the type.")]
        public bool Templates { get; set; }

        [FormUpdatable]
        [Display(Name = "Enable Gallery", Description = "Enable the image gallery uploads.")]
        public bool Gallery { get; set; }

        [FormUpdatable]
        [Display(Name = "Template Folder", Description = "Choose the folder for templates for this content type.")]
        public string TemplateFolder { get; set; }

        [FormUpdatable]
        [Display(Name = "Url Formatting Type", Description = "Choose how the URLs for this content type are rendered.")]
        public string UrlFormatting { get; set; }

        [FormUpdatable]
        [Display(Name = "Cached By Type", Description = "Cache this type for speedy reuse. Only recommended for items that are rendered a lot, not recommended on large content sets such as news.")]
        public bool CachedByType { get; set; }

        [Display(Name = "Default Page Size", Description = "")]
        public int? DefaultPageSize { get; set; }

        [Display(Name = "Custom Fields Json", Description = "")]
        public string CustomFieldsJson { get; set; }


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

