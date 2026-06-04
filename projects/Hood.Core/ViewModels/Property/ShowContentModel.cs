using System.Collections.Generic;
using Hood.Models;

namespace Hood.ViewModels
{
    public class ShowPropertyModel
    {
        public List<PropertyListingView> CloseBy { get; set; }
        public List<PropertyListingView> Similar { get; set; }
        public PropertyListingView Property { get; set; }
    }
}
