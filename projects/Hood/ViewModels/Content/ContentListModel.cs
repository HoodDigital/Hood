using Hood.Models.Api;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ContentListModel : IContentModel
    {
        // IContentView
        public ContentType Type { get; set; }
        public string Category { get; set; }
        public PagedList<Content> Recent { get; set; }
        public IEnumerable<ContentCategory> Categories { get; set; }
        public string Search { get; set; }

        // List
        public PagedList<Content> Posts { get; set; }
        public ApplicationUserApi Author { get; set; }
    }
}
