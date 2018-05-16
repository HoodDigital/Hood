using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class TopicModel : PagedList<Topic>, IPageableModel
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
        [FromQuery(Name = "forumId")]
        public int? ForumId { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }

        // Single Stuff
        public Topic Content { get; set; }
        public Topic Previous { get; set; }
        public Topic Next { get; set; }

        // List Stuff
        public ApplicationUser Author { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Category.IsSet() ? "&category=" + Category : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            query += ForumId.HasValue ? "&forumId=" + ForumId : "";
            return query;
        }

    }
}
