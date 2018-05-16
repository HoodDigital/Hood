using Hood.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models
{
    public class Post : BaseEntity<long>
    {
        // Parent
        public int TopicId { get; set; }
        public Topic Topic { get; set; }

        // Reply?
        public long? ReplyId { get; set; }
        public Post Reply { get; set; }

        // Author
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public string AuthorName { get; set; }
        public string AuthorDisplayName { get; set; }
        public string AuthorIp { get; set; }

        // Dates
        public DateTime PostedTime { get; set; }

        // Content
        public string Body { get; set; }
        public string Signature { get; set; }

        // Moderation
        public bool Approved { get; set; }
        public DateTime? ApprovedTime { get; set; }

        public bool Edited { get; set; }
        public string EditReason { get; set; }
        public DateTime? EditedTime { get; set; }
        public string EditedById { get; set; }
        public ApplicationUser EditedBy { get; set; }

        public bool Deleted { get; set; }
        public string DeleteReason { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string DeletedById { get; set; }
        public ApplicationUser DeletedBy { get; set; }

    }
}
