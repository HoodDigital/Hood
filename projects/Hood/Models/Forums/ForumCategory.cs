using Hood.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class ForumCategory : BaseEntity
    {
        public string DisplayName { get; set; }
        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }

        public ForumCategory ParentCategory { get; set; }
        public List<ForumCategory> Children { get; set; }
        public List<ForumCategoryJoin> Forum { get; set; }

        [NotMapped]
        public IEnumerable<ForumCategory> Categories { get; set; }
        [NotMapped]
        public int Count { get; set; }

    }

    public partial class ForumCategoryJoin
    {
        public int CategoryId { get; set; }
        public ForumCategory Category { get; set; }

        public int ForumId { get; set; }
        public Forum Forum { get; set; }
    }

}
