using Hood.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Hood.Models
{
    public partial class ContentCategory :  BaseEntity
    {
        [Display(Name = "Title / Name", Description = "Display name for your category.")]
        public string DisplayName { get; set; }
        [Display(Name = "Url Slug", Description = "Will be used in the url for the category e.g. <code>yourdomain.com/news/category/your-category-slug/</code>")]
        public string Slug { get; set; }
        [Display(Name = "Content Type")]
        public string ContentType { get; set; }
        [Display(Name = "Parent Category", Description = "Is this a sub-category, if so choose which category it goes under.")]
        public int? ParentCategoryId { get; set; }

        public ContentCategory ParentCategory { get; set; }
        public List<ContentCategory> Children { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public List<ContentCategoryJoin> Content { get; set; }

        [NotMapped]
        public IEnumerable<ContentCategory> Categories { get; set; }
        [NotMapped]
        public int Count { get; set; }

    }

    public partial class ContentCategoryJoin
    {
        public int CategoryId { get; set; }
        public ContentCategory Category { get; set; }

        public int ContentId { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public Content Content { get; set; }
    }

}
