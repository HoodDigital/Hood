using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class SubscriptionGroup :  BaseEntity
    {
        [Display(Name = "Title / Name", Description = "Display name for your group.")]
        public string DisplayName { get; set; }
        [Display(Name = "Url Slug", Description = "Will be used in the url for the group e.g. <code>yourdomain.com/subscriptions/create/your-category-slug/</code>")]
        public string Slug { get; set; }
        [Display(Name = "Content Body", Description = "Displayed on the subscription group page, this should be used to inform your users about the subscriptions available.")]
        public string Body { get; set; }

        /// <summary>
        /// Restricted field, used for JSON FeaturedImage.
        /// </summary>
        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public IMediaObject FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(FeaturedImageJson) : MediaBase.Blank; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        /// <summary>
        /// Restricted field, used for JSON Features.
        /// </summary>
        public string FeaturesJson { get; set; }
        [NotMapped]
        public List<SubscriptionFeature> Features
        {
            get => FeaturesJson.IsSet() ? JsonConvert.DeserializeObject<List<SubscriptionFeature>>(FeaturesJson) : new List<SubscriptionFeature>();
            set => FeaturesJson = JsonConvert.SerializeObject(value);
        }

        public List<Subscription> Subscriptions { get; set; }

    }
}
