using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ShowPropertyModel
    {
        public List<PropertyListing> CloseBy { get; set; }
        public List<PropertyListing> Similar { get; set; }
        public PropertyListing Property { get; set; }
    }
}