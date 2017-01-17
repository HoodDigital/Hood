using System.Collections.Generic;

namespace Hood.Models
{
    public interface IContentModel
    {
        ContentType Type { get; set; }
        string Search { get; set; }
        string Category { get; set; }
        PagedList<Content> Recent { get; set; }
        IEnumerable<ContentCategory> Categories { get; set; }
    }
}
