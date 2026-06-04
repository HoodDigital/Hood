using System.Collections.Generic;
using Hood.Models;

namespace Hood.ViewModels
{
    public class HomePageModel
    {
        public List<Content> News { get; set; }
        public List<Content> Testimonial { get; internal set; }
    }
}
