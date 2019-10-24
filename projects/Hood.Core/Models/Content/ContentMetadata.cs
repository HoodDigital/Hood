namespace Hood.Models
{
    public partial class ContentMeta : MetadataBase, IMetadata
    {
        public ContentMeta()
        {
        }

        public ContentMeta(string name, string value, string type = "System.String") : base(name, value, type)
        {
        }

        public int ContentId { get; set; }
        public Content Content { get; set; }
    }
}

