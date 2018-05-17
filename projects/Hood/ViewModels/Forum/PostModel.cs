using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class PostModel : PagedList<Post>, IPageableModel
    {
        // Params
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
        [FromRoute(Name = "slug")]
        public string Slug { get; set; }
        [FromRoute(Name = "id")]
        public int? TopicId { get; set; }
        [FromRoute(Name = "title")]
        public string Title { get; set; }

        public Post Post { get; set; }

        // Single Stuff
        public Topic Topic { get; set; }
        public Topic Previous { get; set; }
        public Topic Next { get; set; }

        public List<Topic> Recent { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }

    }
}
