using Hood.Entities;
using Hood.Extensions;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hood.Models
{
    public class Post : BaseEntity<long>, IEmailSendable
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
        [Required]
        public string Body { get; set; }
        public string Signature { get; set; }

        /// <summary>
        /// Use this version of the body to include HTML line breaks and basic formatting. (If no front end HTML rich text editor is being used for example)
        /// </summary>
        [NotMapped]
        public string HtmlBody
        {
            get
            {
                return Body.Replace(Environment.NewLine, "<br />");
            }
        }

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

        public MailObject WriteToMessage(MailObject message)
        {
            message.AddDiv("<hr /><br />");
            message.AddParagraph("<strong>Post content:</strong>");
            message.AddDiv(HtmlBody);
            message.AddDiv("<br />");
            message.AddParagraph("<strong>Posted: </strong>" + PostedTime.ToDisplay());
            message.AddParagraph("<strong>Posted by: </strong>" + AuthorDisplayName);
            message.AddParagraph("<strong>Posted by (Username): </strong>" + AuthorName);
            message.AddParagraph("<strong>Posted from (IP Address): </strong>" + AuthorIp);
            message.AddDiv("<hr /><br />");
            return message;
        }
    }
}
