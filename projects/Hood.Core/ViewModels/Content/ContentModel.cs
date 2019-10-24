using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ContentModel : PagedList<Content>, IPageableModel
    {
        public ContentModel() : base()
        {
            Status = ContentStatus.Published;
        }

        // Params
        [FromQuery(Name = "category")]
        public string Category { get; set; }
        [FromRoute(Name = "type")]
        public string Type { get; set; }
        [FromQuery(Name = "filter")]
        public string Filter { get; set; }
        [FromQuery(Name = "author")]
        public string AuthorName { get; set; }
        [FromQuery(Name = "status")]
        public ContentStatus? Status { get; set; }
        [FromQuery(Name = "filter")]
        public bool Featured { get; set; }
        [FromQuery(Name = "inline")]
        public bool Inline { get; set; }
        [FromQuery(Name = "categories")]
        public List<string> Categories { get; set; }

        // Sidebar Stuff
        public ContentType ContentType { get; set; }
        public PagedList<Content> Recent { get; set; }

        // Single Stuff
        public Content Content { get; set; }
        public Content Previous { get; set; }
        public Content Next { get; set; }
        public bool EditMode { get; set; }

        // List Stuff
        public ApplicationUser Author { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            if (Categories != null)
                foreach (var cat in Categories)
                {
                    query += "&categories=" + cat;
                }
            query += Category.IsSet() ? "&category=" + Category : "";
            query += Type.IsSet() ? "&type=" + Type : "";
            query += Filter.IsSet() ? "&filter=" + Filter : "";
            query += AuthorName.IsSet() ? "&author=" + AuthorName : "";
            query += Status.HasValue ? "&status=" + Status : "";
            query += Inline ? "&inline=" + Inline : "";
            return query;
        }
    }
}
