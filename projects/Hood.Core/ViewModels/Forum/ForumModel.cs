using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ForumModel : PagedList<Forum>, IPageableModel
    {
        // Params
        [FromQuery(Name = "category")]
        public string Category { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }

        // Sidebar Stuff
        public List<Forum> Recent { get; set; }
        [FromQuery(Name = "categories")]
        public List<string> Categories { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            if (Categories != null)
                foreach (var cat in Categories)
                {
                    query += "&categories=" + cat;
                }
            query += Category.IsSet() ? "&category=" + Category : "";
            query += AuthorName.IsSet() ? "&author=" + AuthorName : "";
            return query;
        }

    }
}
