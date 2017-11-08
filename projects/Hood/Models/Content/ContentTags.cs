using Hood.Entities;
using Hood.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class ContentTag : ContentTag<HoodIdentityUser> { }
    public class ContentTag<TUser> : BaseEntity where TUser : IHoodUser
    {
        [Key]
        public string Value { get; set; }
        public List<ContentTagJoin<TUser>> Content { get; set; }
    }

    public class ContentTagJoin : ContentTagJoin<HoodIdentityUser> { }
    public class ContentTagJoin<TUser> where TUser : IHoodUser
    {
        public int ContentId { get; set; }
        public Content<TUser> Content { get; set; }

        public string TagId { get; set; }
        public ContentTag<TUser> Tag { get; set; }
    }
}
