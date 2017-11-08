using Hood.Interfaces;

namespace Hood.Models
{
    public class PropertyMeta : PropertyMeta<HoodIdentityUser>
    {
        public PropertyMeta(string name, string value, string type = "System.String")
            : base(name, value, type)
        {
        }
    }

    public partial class PropertyMeta<TUser> : IMetadata where TUser : IHoodUser
    {
        public int Id { get; set; }
        public string BaseValue { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int PropertyId { get; set; }
        public PropertyListing<TUser> Property { get; set; }

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

