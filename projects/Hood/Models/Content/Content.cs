using Hood.Core;
using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public partial class Content : BaseEntity
    {
        // Content
        [Required]
        public string Title { get; set; }

        [Required]
        public string Excerpt { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }

        // Parent Content
        public int? ParentId { get; set; }

        // Dates
        public DateTime PublishDate { get; set; }

        // Content Type
        public string ContentType { get; set; }
        [NotMapped]
        public ContentType Type { get; set; }

        // Publish Status
        public int Status { get; set; }

        // Creator/Editor
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }

        // Logs and notes
        public string UserVars { get; set; }
        public string Notes { get; set; }
        public string SystemNotes { get; set; }

        // View and Sharecounts
        public int Views { get; set; }
        public int ShareCount { get; set; }

        // Settings
        public bool AllowComments { get; set; }
        public bool Public { get; set; }
        public bool Featured { get; set; }

        // MVVM Helpers
        [NotMapped]
        public string PublishDatePart
        {
            get
            {
                return PublishDate.ToShortDateString();
            }
            set
            {
                    DateTime dt = DateTime.Now;
                    if (DateTime.TryParse(value, out dt))
                    {
                    PublishDate = new DateTime(dt.Year, dt.Month, dt.Day, PublishDate.Hour, PublishDate.Minute, PublishDate.Second);
                    }
            }
        }
        [NotMapped]
        public int PublishHours
        {
            get
            {
                return PublishDate.Hour;
            }
            set
            {
                PublishDate = new DateTime(PublishDate.Year, PublishDate.Month, PublishDate.Day, value, PublishDate.Minute, PublishDate.Second);
            }
        }
        [NotMapped]
        public int PublishMinutes
        {
            get
            {
                return PublishDate.Minute;
            }
            set
            {
                PublishDate = new DateTime(PublishDate.Year, PublishDate.Month, PublishDate.Day, PublishDate.Hour, value, PublishDate.Second);
            }
        }

        // Formatted Members
        public string StatusString
        {
            get
            {
                switch ((Enums.Status)Status)
                {
                    case Enums.Status.Published:
                        if (PublishDate > DateTime.Now)
                            return "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                        else
                            return "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                    case Enums.Status.Draft:
                    default:
                        return "Draft";
                    case Enums.Status.Archived:
                        return "Archived";
                    case Enums.Status.Deleted:
                        return "Deleted";
                }
            }
        }
        public bool PublishPending
        {
            get
            {
                if (Status == (int)Enums.Status.Published)
                    return PublishDate > DateTime.Now;
                else
                    return false;
            }
        }
        public string Url
        {
            get
            {
                var siteSettings = Engine.Current.Resolve<ISettingsRepository>();
                ContentSettings _contentSettings = siteSettings.GetContentSettings();
                ContentType type = _contentSettings.GetContentType(ContentType);

                if (type == null)
                    return string.Format("/{0}/{1}", ContentType, Id);

                if (type.BaseName == "Page")
                {
                    if (IsHomepage)
                        return "/";
                    return string.Format("/{0}", Slug);
                }
                switch (type.UrlFormatting)
                {
                    case "news-title":
                        return string.Format("/{0}/{1}/{2}", ContentType, Id, Title.ToSeoUrl());
                    case "news":
                        return string.Format("/{0}/{1}/{2}", ContentType, Id, Slug);
                    default:
                        return string.Format("/{0}/{1}", ContentType, Id);
                }
            }
        }
        public bool IsHomepage
        {
            get
            {
                var siteSettings = Engine.Current.Resolve<ISettingsRepository>();
                BasicSettings basicSettings = siteSettings.GetBasicSettings();
                return Id == basicSettings.Homepage;
            }
        }
        public string GetImageStyle(string imageType = "Featured")
        {
            string align = GetMetaValue(string.Format("Settings.Image.{0}.Align", imageType));
            string fit = GetMetaValue(string.Format("Settings.Image.{0}.Fit", imageType));
            string bg = GetMetaValue(string.Format("Settings.Image.{0}.Background", imageType));
            return string.Format("{0}{1}{2}",
                !string.IsNullOrEmpty(align) ? "background-position:" + align + ";" : "",
                !string.IsNullOrEmpty(fit) ? "background-size:" + fit + ";" : "",
                !string.IsNullOrEmpty(bg) ? "background-color:" + bg + ";" : "");
        }

        [NotMapped]
        public IEnumerable<ContentCategory> AllowedCategories { get; set; }
        // Author 
        public string AuthorId { get; set; }
        [JsonConverter(typeof(ApplicationUserJsonConverter))]
        public ApplicationUser Author { get; set; }

        public List<ContentCategoryJoin> Categories { get; set; }
        public List<ContentMeta> Metadata { get; set; }
        public List<ContentMedia> Media { get; set; }
        public List<ContentTagJoin> Tags { get; set; }

        public string FeaturedImageJson { get; set; }
        [NotMapped]
        [JsonConverter(typeof(MediaObjectJsonConverter))]
        public IMediaObject FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(FeaturedImageJson) : ContentMedia.Blank; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        public string ShareImageJson { get; set; }
        [NotMapped]
        [JsonConverter(typeof(MediaObjectJsonConverter))]
        public IMediaObject ShareImage
        {
            get { return ShareImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(ShareImageJson) : ContentMedia.Blank; }
            set { ShareImageJson = JsonConvert.SerializeObject(value); }
        }

        [NotMapped]
        public string TagString
        {
            get
            {
                if (Tags != null)
                    return string.Join(",", Tags.Select(x => x.Tag?.Value).ToArray());
                return "";
            }
        }
        public bool IsInCategory(int categoryId)
        {
            if (Categories == null)
                return false;
            return Categories.Select(c => c.Category.Id).Contains(categoryId);
        }
        public void AddTag(string value)
        {
            if (Tags == null)
                Tags = new List<ContentTagJoin>();
            if (!Tags.Select(c => c.Tag.Value).Contains(value))
            {
                Tags.Add(new ContentTagJoin() { ContentId = Id, TagId = value });
            }
        }
        public void RemoveTag(string value)
        {
            if (Tags == null)
                Tags = new List<ContentTagJoin>();
            var tag = Tags.Where(c => c.Tag.Value == value).FirstOrDefault();
            if (tag != null)
            {
                Tags.Remove(tag);
            }
        }
        public ContentMeta GetMeta(string name)
        {
            ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
            if (cm == null)
                return new ContentMeta()
                {
                    BaseValue = null,
                    Name = name,
                    Type = null
                };
            return cm;
        }
        public string GetMetaValue(string name)
        {
            ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
            if (cm != null)
                return cm.GetStringValue();
            return null;
        }
        public void UpdateMeta<T>(string name, T value)
        {
            if (Metadata != null)
            {
                ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    cm.Set(value);
                }
            }
        }
        public void AddMeta(ContentMeta value)
        {
            if (Metadata != null)
            {
                Metadata.Add(value);
            }
        }
        public bool HasMeta(string name)
        {
            if (Metadata != null)
            {
                ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                    return true;
            }
            return false;
        }
        public Content Clean()
        {
            Author.Content = null;
            Author.Properties = null;
            Author.Addresses = null;
            return this;
        }
    }
}

