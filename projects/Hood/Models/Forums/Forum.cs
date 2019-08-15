using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public class Forum : ForumEntity
    {
        // Forum
        [Required]
        [Display(Name = "Title", Description = "The title for the forum.")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description", Description = "This description will be shown in forum list views.")]
        public string Description { get; set; }

        [Display(Name = "Description", Description = "This body will be shown on the forum's main page.")]
        public string Body { get; set; }
        public string Slug { get; set; }

        [Display(Name = "URL Slug", Description = "Do not start your url slug with reserved words as they will not reach this page.<br />These include: <strong>account, about, store, admin, api, services.</strong>")]
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

        [Display(Name = "Topic Moderation", Description = "If set, all topics will require approval by a moderator.")]
        public bool RequireTopicModeration { get; set; }
        [Display(Name = "Post & Reply Moderation", Description = "If set, all posts & replies will require approval by a moderator.")]
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
