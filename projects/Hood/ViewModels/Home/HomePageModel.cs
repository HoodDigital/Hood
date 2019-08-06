using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class HomePageModel
    {
        public List<Content> News { get; set; }
        public List<Content> Testimonial { get; internal set; }
    }
}