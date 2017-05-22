using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public partial class Content : IContent<ContentMeta, SiteMedia>
    {
        // Content
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }


        // Parent Content
        public int? ParentId { get; set; }

        // Author 
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        // Dates
        public DateTime PublishDate { get; set; }

        // Content Type
        public string ContentType { get; set; }

        // Publish Status
        public int Status { get; set; }

        // Featured Images
        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public SiteMedia FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<SiteMedia>(FeaturedImageJson) : null; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

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

        public List<ContentCategoryJoin> Categories { get; set; }

        public List<ContentMeta> Metadata { get; set; }
        public List<ContentMedia> Media { get; set; }

        public List<ContentTagJoin> Tags { get; set; }

        [NotMapped]
        public IEnumerable<ContentCategory> AllowedCategories { get; set; }

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


        public bool IsInCategory(string category)
        {
            if (Categories == null)
                return false;
            return Categories.Select(c => c.Category.DisplayName).Contains(category);
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
    }
}

