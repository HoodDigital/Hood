using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ForumModel : PagedList<Forum>, IPageableModel
    {
        // Params
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
        [FromQuery(Name = "category")]
        public string Category { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }

        // Sidebar Stuff
        public IEnumerable<ForumCategory> Categories { get; set; }

        // Single Stuff
        public Forum Content { get; set; }
        public Forum Previous { get; set; }
        public Forum Next { get; set; }

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
