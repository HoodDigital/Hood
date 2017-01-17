using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{ 
    public class ContentTag
    {
        [Key]
        public string Value { get; set; }
        public List<ContentTagJoin> Content { get; set; }
    }

    public class ContentTagJoin 
    {
        public int ContentId { get; set; }
        public Content Content { get; set; }

        public string TagId { get; set; }
        public ContentTag Tag { get; set; }
    }
}
