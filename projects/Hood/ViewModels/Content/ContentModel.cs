using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ContentModel : PagedList<Content>, IPageableModel
    {
        // Params
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
        [FromQuery(Name = "category")]
        public string Category { get; set; }
        [FromRoute(Name = "type")]
        public string Type { get; set; }
        [FromQuery(Name = "filter")]
        public string Filter { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }

        // Sidebar Stuff
        public ContentType ContentType { get; set; }
        public PagedList<Content> Recent { get; set; }
        public IEnumerable<ContentCategory> Categories { get; set; }

        // Single Stuff
        public Content Content { get; set; }
        public Content Previous { get; set; }
        public Content Next { get; set; }
        public bool EditMode { get; set; }

        // List Stuff
        public ApplicationUser Author { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Category.IsSet() ? "&category=" + Category : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }

    }
}
