using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ShowPropertyModel
    {
        public List<PropertyListingView> CloseBy { get; set; }
        public List<PropertyListingView> Similar { get; set; }
        public PropertyListingView Property { get; set; }
    }
}