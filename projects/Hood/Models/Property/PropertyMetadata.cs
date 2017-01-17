using Hood.Interfaces;

namespace Hood.Models
{
    public partial class PropertyMeta : IMetadata
    {
        public int Id { get; set; }
        public string BaseValue { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int PropertyId { get; set; }
        public PropertyListing Property { get; set; }

        public PropertyMeta()
        {
        }

        public PropertyMeta(string name, string value, string type = "System.String")
        {
            Type = type;
            Name = name;
            BaseValue = value;
        }
    }

}

