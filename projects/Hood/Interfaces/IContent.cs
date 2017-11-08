using Hood.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Interfaces
{
    public interface IContent<TMetadata, TMediaItem, TUser> : IMetaObect<TMetadata>
        where TMetadata : IMetadata
        where TMediaItem : IMediaObject
        where TUser : IHoodUser
    {

        // Content
        int Id { get; set; }

        // Dates
        DateTime PublishDate { get; set; }

        // Publish Status
        int Status { get; set; }

        // Featured Image
        string FeaturedImageJson { get; set; }
        [NotMapped]
        TMediaItem FeaturedImage { get; set; }

        // Creator/Editor
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime LastEditedOn { get; set; }
        string LastEditedBy { get; set; }


        // Logs and notes
        string UserVars { get; set; }
        string Notes { get; set; }
        string SystemNotes { get; set; }

        // View and Sharecounts
        int Views { get; set; }
        int ShareCount { get; set; }

        // Settings
        bool AllowComments { get; set; }
        bool Public { get; set; }

        List<ContentCategoryJoin> Categories { get; set; }
        List<TMediaItem> Media { get; set; }
        List<ContentTagJoin> Tags { get; set; }

        [NotMapped]
        string TagString{get;}
        bool IsInCategory(string category);
        void AddTag(string value);
        void RemoveTag(string value);

    }
}
