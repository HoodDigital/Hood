using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class PostModel : PagedList<Post>, IPageableModel
    {
        // Params
        [FromRoute(Name = "slug")]
        public string Slug { get; set; }
        [FromRoute(Name = "topicId")]
        public int? TopicId { get; set; }
        [FromRoute(Name = "title")]
        public string Title { get; set; }
        [FromQuery(Name = "highlight")]
        public long? HighlightId { get; set; }
        [FromQuery(Name = "edit")]
        public long? EditId { get; set; }
        [FromQuery(Name = "reply")]
        public long? ReplyId { get; set; }

        public Post Post { get; set; }

        // Single Stuff
        public Topic Topic { get; set; }
        public Topic Previous { get; set; }
        public Topic Next { get; set; }

        public List<Topic> Recent { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            return query;
        }

    }
}
