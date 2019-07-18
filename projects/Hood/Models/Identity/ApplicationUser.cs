using Hood.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public partial class ApplicationUser : UserProfileBase
    {
        #region Related Tables
        [JsonIgnore]
        public List<ApiKey> ApiKeys { get; set; }
        [JsonIgnore]
        public List<Content> Content { get; set; }
        [JsonIgnore]
        public List<Forum> Forums { get; set; }
        [JsonIgnore]
        public List<Topic> Topics { get; set; }
        [JsonIgnore]
        public List<PropertyListing> Properties { get; set; }
        [JsonIgnore]
        public List<UserSubscription> Subscriptions { get; set; }
        [JsonIgnore]
        public List<Post> Posts { get; set; }
        [JsonIgnore]
        public List<Post> EditedPosts { get; set; }
        [JsonIgnore]
        public List<Post> DeletedPosts { get; set; }
        #endregion

        #region Profile Get/Set 
        /// <summary>
        /// A Get/Set to allow the loading from or saving to a IUserProfile.
        /// </summary>
        [NotMapped]
        public IUserProfile Profile
        {
            get { return this as IUserProfile; }
            set { value.CopyProperties(this); }
        }
        #endregion
        
    }


}
