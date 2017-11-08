using System.Collections.Generic;

namespace Hood.Models
{
    public class ShowPropertyModel<TUser> where TUser : IHoodUser
    {
        public List<PropertyListing<TUser>> CloseBy { get; set; }
        public List<PropertyListing<TUser>> Similar { get; set; }
        public PropertyListing<TUser> Property { get; set; }
    }
}