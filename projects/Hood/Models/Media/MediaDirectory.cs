using Hood.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class MediaDirectory :  BaseEntity
    {
        [Display(Name = "Directory Name", Description = "Display name for your directory.")]
        public string DisplayName { get; set; }
        [Display(Name = "Url Slug", Description = "Will be used in the url for the directory.")]
        public string Slug { get; set; }
        [Display(Name = "Directory Type", Description = "Is this a system, site or user directory?")]
        public DirectoryType Type { get; set; }
        [Display(Name = "Owner Id", Description = "The owner of this directory.")]
        public string OwnerId { get; set; }

        [Display(Name = "Parent Directory", Description = "Is this a sub-directory, if so choose which directory it goes under.")]
        public int? ParentId { get; set; }
        public MediaDirectory Parent { get; set; }
        public List<MediaDirectory> Children { get; set; }
        public List<MediaObject> Media { get; set; }

        [NotMapped]
        public IEnumerable<MediaDirectory> TopLevelDirectories { get; set; }
    }

    public enum DirectoryType
    {
        Site, 
        User,
        System
    }
}
