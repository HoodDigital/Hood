using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class ForumSettings : ForumAccessEntity
    {
        public ForumSettings()
        {
        }

        [Display(Name = "Enable Forums")]
        public bool Enabled { get; set; }
        [Display(Name = "Forum Area Name", Description = "Rename your forums, you may wish to call them something else, such as 'Discussions' etc.")]
        public string ForumAreaName { get; set; }
        [Display(Name = "Forum Area Name (Plural)", Description = "Rename your forums, you may wish to call them something else, such as 'Discussions' etc.")]
        public string ForumAreaNamePlural { get; set; }
    }

}
