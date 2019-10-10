using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class TopicModel : PagedList<Topic>, IPageableModel
    {
        // Params
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

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            return query;
        }

    }
}
