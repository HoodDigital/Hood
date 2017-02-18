using CodeComb.HtmlAgilityPack;
using Hood.Extensions;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hood.Models.Api
{
    public class ContentApi
    {
        // Content
        public int Id { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Body { get; set; }
        public string Tags { get; set; }
        public string Slug { get; set; }

        // Parent Content
        public int ParentId { get; set; }

        // Author 
        public string AuthorId { get; set; }

        // Dates
        public DateTime PublishDate { get; set; }

        // Content Type
        public string ContentType { get; set; }

        // Publish Status
        public int Status { get; set; }

        // Featured Images
        public int? FeaturedImageId { get; set; }
        public int? BannerImageId { get; set; }

        // Creator/Editor
        public DateTime CreatedOn { get; set; }
        public string CreatedById { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedById { get; set; }

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

        public ApplicationUserApi Author { get; set; }
        public MediaApi FeaturedImage { get; set; }
        public string CreatedBy { get; set; }
        public string LastEditedBy { get; set; }

        public List<string> Categories { get; set; }
        public IList<MetaDataApi<ContentMeta>> Meta { get; set; }

        // MVVM Helpers
        public int PublishHour { get; set; }
        public int PublishMinute { get; set; }
        public string PublishDatePart { get; set; }

        // Formatted Members
        public string StatusString { get; set; }
        public bool PublishPending { get; set; }
        public string Url { get; set; }
        public List<MediaApi> Media { get; set; }
        public bool IsHomepage { get; set; }

        public ContentApi()
        {
        }

        public ContentApi(Content post, ISiteConfiguration settings = null)
        {
            if (post == null)
                return;
            post.CopyProperties(this);

            var mediaSettings = settings.GetMediaSettings();

            IsHomepage = Id == settings.GetBasicSettings().Homepage;

            if (post.FeaturedImage != null)
                FeaturedImage = new MediaApi(post.FeaturedImage);
            else
                FeaturedImage = MediaApi.Blank(mediaSettings);

            if (post.Author != null)
                Author = new ApplicationUserApi(post.Author, settings);

            if (post.CreatedBy != null)
                CreatedBy = post.CreatedBy;
            if (post.LastEditedBy != null)
                LastEditedBy = post.LastEditedBy;

            if (post.Media == null || post.Media.Count == 0)
                Media = new List<MediaApi>();
            else
                Media = post.Media.Select(c => new MediaApi(c)).ToList();

            if (post.Categories == null || post.Categories.Count == 0)
                Categories = new List<string>();
            else
                Categories = post.Categories.Select(c => c.Category.DisplayName).ToList();

            PublishPending = false;
            PublishDatePart = PublishDate.ToShortDateString();
            PublishHour = PublishDate.Hour;
            PublishMinute = PublishDate.Minute;

            if (post.Metadata == null || post.Metadata.Count == 0)
                Meta = new List<MetaDataApi<ContentMeta>>();
            else
                Meta = post.Metadata.Select(cm => new MetaDataApi<ContentMeta>(cm)).ToList();
            ContentSettings _contentSettings = settings.GetContentSettings();
            ContentType type = _contentSettings.GetContentType(ContentType);
            switch (type.UrlFormatting)
            {
                case "news-title":
                    Url = string.Format("/{0}/{1}/{2}", post.ContentType, post.Id, post.Title.ToSeoUrl());
                    break;
                case "news":
                    Url = string.Format("/{0}/{1}/{2}", post.ContentType, post.Id, post.Slug);
                    break;
                default:
                    Url = string.Format("/{0}/{1}", post.ContentType, post.Id);
                    break;
            }
            if (type.BaseName == "Page")
            {
                Url = string.Format("/{0}", post.Slug);
            }
            if (Status == 1)
            {
                StatusString = "Draft <span>(Provisional publish date " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString() + ")</span>";
            }
            else
            {
                if (PublishDate > DateTime.Now)
                {
                    PublishPending = true;
                    StatusString = "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                }
                else
                {
                    StatusString = "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                }
            }
        }

        public static string FormatBody(string body, List<Content> blocks, ICollection<ContentMeta> metadata)
        {
            HtmlDocument inputDoc = new HtmlDocument();
            inputDoc.LoadHtml(body);
            HtmlDocument outputDoc = inputDoc;
            foreach (HtmlNode node in inputDoc.DocumentNode.ChildNodes.Traverse(i => i.Attributes.Where(a => a.Name == "class" && a.Value.Contains("hoodcontent-replace")).Count() > 0))
            {
                int id = node.GetAttributeValue("id", 0);
                if (id != 0)
                {
                    // check blocks for this id content block
                    Content block = blocks.Where(c => c.Id == id).FirstOrDefault();
                    if (block != null)
                    {
                        // found one! now insert the content into the node.
                        HtmlDocument blockDoc = new HtmlDocument();
                        blockDoc.LoadHtml(block.Body);
                        // then rename the node to div.
                        StringWriter blockWriter = new StringWriter();
                        blockDoc.Save(blockWriter);
                        node.InnerHtml = blockWriter.ToString();
                        node.Name = "div";
                        node.SetAttributeValue("class", "");
                    }
                }
            }
            StringWriter sw = new StringWriter();
            inputDoc.Save(sw);
            return sw.ToString();
        }


        public MetaDataApi<ContentMeta> GetMeta(string name)
        {
            MetaDataApi<ContentMeta> cm = Meta.FirstOrDefault(p => p.Name == name);
            if (cm != null)
                return cm;
            return new MetaDataApi<ContentMeta>();
        }

        public string GetMetaValue(string name)
        {
            MetaDataApi<ContentMeta> cm = Meta.FirstOrDefault(p => p.Name == name);
            if (cm != null)
                return cm.ToString();
            return null;
        }

        public string GetImageStyle(string imageType = "Featured")
        {
            string align = GetMeta(string.Format("Settings.Image.{0}.Align", imageType)).ToString();
            string fit = GetMeta(string.Format("Settings.Image.{0}.Fit", imageType)).ToString();
            string bg = GetMeta(string.Format("Settings.Image.{0}.Background", imageType)).ToString();
            return string.Format("{0}{1}{2}",
                !string.IsNullOrEmpty(align) ? "background-position:" + align + ";" : "",
                !string.IsNullOrEmpty(fit) ? "background-size:" + fit + ";" : "",
                !string.IsNullOrEmpty(bg) ? "background-color:" + bg + ";" : "");
        }
    }

}
