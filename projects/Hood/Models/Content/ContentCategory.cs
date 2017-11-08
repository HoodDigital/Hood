using Hood.Entities;
using Hood.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class ContentCategory : ContentCategoryBase
    {
        public ContentCategory ParentCategory { get; set; }
        public List<ContentCategory> Children { get; set; }
        public List<ContentCategoryJoin> Content { get; set; }
        [NotMapped]
        public IEnumerable<ContentCategory> Categories { get; set; }
    }
    public partial class ContentCategory<TUser> : ContentCategoryBase where TUser : IHoodUser
    {
        public ContentCategory<TUser> ParentCategory { get; set; }
        public List<ContentCategory<TUser>> Children { get; set; }
        public List<ContentCategoryJoin<TUser>> Content { get; set; }
        [NotMapped]
        public IEnumerable<ContentCategory<TUser>> Categories { get; set; }
    }
    public abstract class ContentCategoryBase : BaseEntity
    {
        public string DisplayName { get; set; }
        public string Slug { get; set; }
        public string ContentType { get; set; }
        public int? ParentCategoryId { get; set; }

        [NotMapped]
        public int Count { get; set; }
    }

    public partial class ContentCategoryJoin : ContentCategoryJoinBase
    {
        public ContentCategory Category { get; set; }
        public Content Content { get; set; }
    }
    public class ContentCategoryJoin<TUser> where TUser : IHoodUser
    {
        public ContentCategory<TUser> Category { get; set; }
        public Content<TUser> Content { get; set; }
    }
    public abstract class ContentCategoryJoinBase 
    {
        public int CategoryId { get; set; }
        public int ContentId { get; set; }
    }

}
