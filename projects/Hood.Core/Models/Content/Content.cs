﻿using Hood.Attributes;
using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{

    public class Content : BaseContent
    {

    }

    public partial class BaseContent : BaseEntity
    {
        // Content
        [Required]
        [FormUpdatable]
        public string Title { get; set; }

        [Required]
        [FormUpdatable]
        public string Excerpt { get; set; }
        [FormUpdatable]
        public string Body { get; set; }

        [FormUpdatable]
        [Display(Name = "URL Slug", Description = "Do not start your url slug with reserved words as they will not reach this page.<br />These include: <strong>account, about, store, admin, api, services.</strong>")]
        public string Slug { get; set; }


        // Parent Content
        public int? ParentId { get; set; }

        // Dates
        [FormUpdatable]
        [Display(Name = "Publish Date", Description = "The content will only appear on the site after this date, when set to published.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime PublishDate { get; set; }

        // Content Type
        public string ContentType { get; set; }
        [NotMapped]
        public ContentType Type { get; set; }

        // Publish Status
        [FormUpdatable]
        public ContentStatus Status { get; set; }

        // Creator/Editor
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }

        // View and Sharecounts
        public int Views { get; set; }
        public int ShareCount { get; set; }

        // Settings
        public bool AllowComments { get; set; }
        public bool ShowAuthor
        {
            get
            {
                if (Type == null || Type.HideAuthor)
                {
                    return false;
                }
                return true;
            }
        }

        [FormUpdatable]
        [Display(Name = "Protected", Description = "This will only be available to logged in users.")]
        public bool Public { get; set; }

        [FormUpdatable]
        [Display(Name = "Featured Content", Description = "This will appear in the featured lists on which can be displayed in templates.")]
        public bool Featured { get; set; }

        // Formatted Members
        public string StatusString
        {
            get
            {
                switch ((Enums.ContentStatus)Status)
                {
                    case ContentStatus.Published:
                        if (PublishDate > DateTime.UtcNow)
                            return "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                        else
                            return "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                    case ContentStatus.Draft:
                    default:
                        return "Draft";
                    case ContentStatus.Archived:
                        return "Archived";
                    case ContentStatus.Deleted:
                        return "Deleted";
                }
            }
        }
        public bool PublishPending
        {
            get
            {
                if (Status == ContentStatus.Published)
                    return PublishDate > DateTime.UtcNow;
                else
                    return false;
            }
        }
        public string Url
        {
            get
            {
                ContentType type = Engine.Settings.Content.GetContentType(ContentType);

                if (type == null || type.IsUnknown)
                {
                    var linkGenerator = Engine.Services.Resolve<Microsoft.AspNetCore.Routing.LinkGenerator>();
                    return linkGenerator.GetPathByAction("Show", "Home", new { id = Id });
                }

                if (type.BaseName == "Page")
                {
                    if (IsHomepage)
                        return "/";
                    return string.Format("/{0}", Slug);
                }

                switch (type.UrlFormatting)
                {
                    case "news-title":
                        return string.Format("/{0}/{1}/{2}", type.Slug, Id, Title.ToSeoUrl());
                    case "news":
                        return string.Format("/{0}/{1}/{2}", type.Slug, Id, Slug);
                    default:
                        var linkGenerator = Engine.Services.Resolve<Microsoft.AspNetCore.Routing.LinkGenerator>();
                        return linkGenerator.GetPathByAction("Show", "Home", new { id = Id });
                }
            }
        }
        public bool IsHomepage
        {
            get
            {
                BasicSettings basicSettings = Engine.Settings.Basic;
                return Id == basicSettings.Homepage;
            }
        }
        public string GetImageStyle(string imageType = "Featured")
        {
            string align = GetMetaValue<string>(string.Format("Settings.Image.{0}.Align", imageType));
            string fit = GetMetaValue<string>(string.Format("Settings.Image.{0}.Fit", imageType));
            string bg = GetMetaValue<string>(string.Format("Settings.Image.{0}.Background", imageType));
            return string.Format("{0}{1}{2}",
                !string.IsNullOrEmpty(align) ? "background-position:" + align + ";" : "",
                !string.IsNullOrEmpty(fit) ? "background-size:" + fit + ";" : "",
                !string.IsNullOrEmpty(bg) ? "background-color:" + bg + ";" : "");
        }

        [NotMapped]
        public IEnumerable<ContentCategory> AllowedCategories { get; set; }
        // Author 
        [FormUpdatable]
        [Display(Name = "Author/Owner", Description = "The author or creator of this content.")]
        public string AuthorId { get; set; }

        public List<ContentCategoryJoin> Categories { get; set; }
        public List<ContentMeta> Metadata { get; set; }
        public List<ContentMedia> Media { get; set; }

        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public IMediaObject FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(FeaturedImageJson) : ContentMedia.Blank; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        public string ShareImageJson { get; set; }
        [NotMapped]
        public IMediaObject ShareImage
        {
            get { return ShareImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(ShareImageJson) : ContentMedia.Blank; }
            set { ShareImageJson = JsonConvert.SerializeObject(value); }
        }

        public bool IsInCategory(int categoryId)
        {
            if (Categories == null)
                return false;
            return Categories.Select(c => c.Category.Id).Contains(categoryId);
        }

        #region Edit View Model Stuff
        [NotMapped]
        public Dictionary<string, string> Templates { get; set; }
        [NotMapped]
        public IList<IUserProfile> Authors { get; set; }
        #endregion

        #region Metadata
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
        public T GetMetaValue<T>(string name)
        {
            ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
            if (cm != null)
                return cm.GetValue<T>();
            return default;
        }
        public void UpdateMeta(string name, string value)
        {
            if (Metadata != null)
            {
                ContentMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    cm.SetValue(value);
                }
            }
        }
        public void AddMeta(string name, string value, string metaType = "System.String")
        {

            var newMeta = new ContentMeta()
            {
                Name = name,
                Type = metaType,
                ContentId = Id
            };
            newMeta.SetValue(value);
            if (Metadata != null)
            {
                Metadata.Add(newMeta);
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
        #endregion
    }
}

