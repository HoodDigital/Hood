using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ContentModel : PagedList<ContentView>, IPageableModel
    {
        public ContentModel() : base()
        {
        }

        public override int PageSize
        {
            get
            {
                if (ContentType != null && ContentType.DefaultPageSize.HasValue)
                {
                    return ContentType.DefaultPageSize.Value;
                }
                else
                    return base.PageSize;
            }
            set => base.PageSize = value;
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
        public PagedList<ContentView> Recent { get; set; }

        // Single Stuff
        public ContentView Content { get; set; }
        public ContentView Previous { get; set; }
        public ContentView Next { get; set; }
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
            query += Category.IsSet() ? "&category=" + System.Net.WebUtility.UrlEncode(Category) : "";
            query += Type.IsSet() ? "&type=" + System.Net.WebUtility.UrlEncode(Type) : "";
            query += Filter.IsSet() ? "&filter=" + System.Net.WebUtility.UrlEncode(Filter) : "";
            query += AuthorName.IsSet() ? "&author=" + System.Net.WebUtility.UrlEncode(AuthorName) : "";
            query += Status.HasValue ? "&status=" + Status : "";
            query += Inline ? "&inline=" + Inline : "";
            return query;
        }
    }
}
