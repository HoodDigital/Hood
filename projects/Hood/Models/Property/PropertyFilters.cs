namespace Hood.Models
{
    public class PropertyFilters : ListFilters
    {
        public string Type { get; set; }
        public string Transaction { get; set; }
        public string PlanningType { get; set; }
        public string Location { get; set; }
        public int? Bedrooms { get; set; }
        public int? MaxRent { get; set; }
        public int? MinRent { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPremium { get; set; }
        public int? MinPremium { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Agent { get; set; }
        public string Status { get; set; }
    }
}