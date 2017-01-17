using Hood.Models.Api;
using System.Collections.Generic;

namespace Hood.Models
{
    public class HomePageModel
    {
        public List<ContentApi> News { get; set; }
        public List<ContentApi> Testimonial { get; internal set; }
    }
}