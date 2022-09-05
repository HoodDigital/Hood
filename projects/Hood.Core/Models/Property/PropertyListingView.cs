using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class PropertyListingView : BasePropertyListing, IName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarJson { get; set; }        
        [NotMapped]
        public virtual IMediaObject Avatar
        {
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(AvatarJson) : MediaObject.BlankAvatar; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }
        public string AgentEmail { get; set; }
        public bool Anonymous { get; set; }
        public string DisplayName { get; set; }
        public virtual string AuthorVars { get; set; }
        [NotMapped]
        public virtual Dictionary<string, string> AuthorMetadata
        {
            get { return AuthorVars.IsSet() ? JsonConvert.DeserializeObject<Dictionary<string, string>>(AuthorVars) : new Dictionary<string, string>(); }
            set { AuthorVars = JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public virtual string this[string key]
        {
            get
            {
                if (AuthorMetadata.ContainsKey(key))
                    return AuthorMetadata[key];
                return null;
            }
        }
        [NotMapped]
        public virtual string JobTitle { get => this[nameof(JobTitle)]; }
        [NotMapped]
        public virtual string WebsiteUrl { get => this[nameof(WebsiteUrl)]; }
        [NotMapped]
        public virtual string Twitter { get => this[nameof(Twitter)]; }
        [NotMapped]
        public virtual string Facebook { get => this[nameof(Facebook)]; }
        [NotMapped]
        public virtual string Instagram { get => this[nameof(Instagram)]; }
        [NotMapped]
        public virtual string LinkedIn { get => this[nameof(LinkedIn)]; }
    }

}

