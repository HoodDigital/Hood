namespace Hood.Models
{
    public class PropertyMeta : MetadataBase
    {
        public PropertyMeta()
        {
        }

        public int PropertyId { get; set; }
        public PropertyListing Property { get; set; }

    }
}

