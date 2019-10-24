using Newtonsoft.Json;
using System.Collections.Generic;

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
        
    }


}
