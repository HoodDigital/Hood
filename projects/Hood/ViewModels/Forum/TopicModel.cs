using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class TopicModel : PagedList<Topic>, IPageableModel
    {
        // Params
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
        [FromRoute(Name = "slug")]
        public string Slug { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }

        public Topic Topic { get; set; }

        // Single Stuff
        public Forum Forum { get; set; }
        public Forum Previous { get; set; }
        public Forum Next { get; set; }

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
