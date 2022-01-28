using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hood.Models
{
    public partial class ApplicationUser : UserProfileBase
    {
        #region Related Tables

        [JsonIgnore]
        public List<Content> Content { get; set; }
        [JsonIgnore]
        public List<PropertyListing> Properties { get; set; }
        #endregion


    }


}
