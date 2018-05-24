using Hood.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hood.Models
{
    public class Topic : ForumEntity
    {
        // Parent
        public int ForumId { get; set; }
        public Forum Forum { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        // Moderation
        public bool Approved { get; set; }
        public DateTime? ApprovedTime { get; set; }

        public int NumPosts { get; set; }
        public int ModeratedPostCount { get; set; }

        public bool AllowReplies { get; set; }
        public List<Post> Posts { get; set; }
    }
}
