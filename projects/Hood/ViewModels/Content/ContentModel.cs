using System.Collections.Generic;

namespace Hood.Models
{
    public class ContentModel : IContentModel
    {
        // IContentView
        public ContentType Type { get; set; }
        public string Category { get; set; }
        public PagedList<Content> Recent { get; set; }
        public IEnumerable<ContentCategory> Categories { get; set; }
        public string Search { get; set; }

        // Single
        public Content Content { get; set; }
        public Content Previous { get; set; }
        public Content Next { get; set; }
        public bool EditMode { get; set; }
    }
}
