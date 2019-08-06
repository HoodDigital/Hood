namespace Hood.Models
{
    public class PropertyMeta : MetadataBase
    {
        public PropertyMeta()
        {
        }

        public PropertyMeta(string name, string value, string type = "System.String") : base(name, value, type)
        {
        }

        public int PropertyId { get; set; }
        public PropertyListing Property { get; set; }

    }
}

