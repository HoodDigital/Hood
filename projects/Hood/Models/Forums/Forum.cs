using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Hood.Models
{
    public class Forum : ForumEntity
    {
        // Forum
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Body { get; set; }
        public string Slug { get; set; }

        public string Url
        {
            get
            {
                return string.Format("/forum/{0}", Slug);
            }
        }

        // Post info
        public int NumTopics { get; set; }
        public int NumPosts { get; set; }
        public int ModeratedPostCount { get; set; }

        public bool RequireTopicModeration { get; set; }
        public bool RequirePostModeration { get; set; }

        // Topics
        public List<Topic> Topics { get; set; }

        public List<ForumCategoryJoin> Categories { get; set; }
        [NotMapped]
        public IEnumerable<ForumCategory> AllowedCategories { get; set; }

        public bool IsInCategory(int categoryId)
        {
            if (Categories == null)
                return false;
            return Categories.Select(c => c.CategoryId).Contains(categoryId);
        }
    }
}
