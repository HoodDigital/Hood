using System.Collections.Generic;

namespace Hood.Models
{
    public class ListPropertyModel
    {
        public PropertyFilters Filters { get; set; }
        public PagedList<PropertyListing> Properties { get; set; }
        public List<string> Types { get; set; }
        public Dictionary<string, string> PlanningTypes { get; set; }
        public List<MapMarker> Locations { get; set; }
        public GeoCoordinate CentrePoint { get; internal set; }
    }
}
