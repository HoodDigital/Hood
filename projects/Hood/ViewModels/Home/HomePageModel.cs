using Hood.Models.Api;
using System.Collections.Generic;

namespace Hood.Models
{
    public class HomePageModel<TUser> where TUser : IHoodUser
    {
        public List<ContentApi<TUser>> News { get; set; }
        public List<ContentApi<TUser>> Testimonial { get; internal set; }
    }
}