using System.Collections.Generic;

namespace Hood.Models
{
    public class ShowPropertyModel
    {
        public List<PropertyListing> CloseBy { get; set; }
        public List<PropertyListing> Similar { get; set; }
        public PropertyListing Property { get; set; }
    }
}