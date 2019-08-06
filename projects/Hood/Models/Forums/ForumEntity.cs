using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class ForumEntity : ForumAccessEntity
    {
        // Author 
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public string AuthorName { get; set; }
        public string AuthorDisplayName { get; set; }
        public string AuthorRoles { get; set; }

        [NotMapped]
        public List<ApplicationUser> Authors { get; set; }

        // Creator/Editor
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }

        // Last posting
        public DateTime? LastPosted { get; set; }
        public int? LastTopicId { get; set; }
        public long? LastPostId { get; set; }
        public string LastUserId { get; set; }
        public string LastUserName { get; set; }
        public string LastUserDisplayName { get; set; }

        // Logs and notes
        public string UserVars { get; set; }
        public string Notes { get; set; }
        public string SystemNotes { get; set; }

        // View and Sharecounts
        public int Views { get; set; }
        public int ShareCount { get; set; }

        // Settings
        public bool Published { get; set; }
        public bool Featured { get; set; }

        // Images
        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public IMediaObject FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(FeaturedImageJson) : MediaObject.Blank; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        public string ShareImageJson { get; set; }
        [NotMapped]
        public IMediaObject ShareImage
        {
            get { return ShareImageJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(ShareImageJson) : MediaObject.Blank; }
            set { ShareImageJson = JsonConvert.SerializeObject(value); }
        }
    }


}
