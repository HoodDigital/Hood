using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class ContentCategory
    {
        public int ContentCategoryId { get; set; }
        public string DisplayName { get; set; }
        public string Slug { get; set; }
        public string ContentType { get; set; }
        public int? ParentCategoryId { get; set; }
        public ContentCategory ParentCategory { get; set; }
        public List<ContentCategory> Children { get; set; }
        public List<ContentCategoryJoin> Content { get; set; }

        [NotMapped]
        public int Count { get; set; }
        [NotMapped]
        public IEnumerable<ContentCategory> Categories { get; set; }
    }

    public class ContentCategoryJoin
    {
        public int CategoryId { get; set; }
        public ContentCategory Category { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
    }

}
