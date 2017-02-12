using System.Collections.Generic;

namespace Hood.Models
{
    public class PagedList<TObject>
    {
        public int Count { get; set; }
        public int? PageSize { get; set; }
        public int Pages { get; set; }
        public int CurrentPage { get; set; }
        public IEnumerable<TObject> Items { get; set; }
    }
}
