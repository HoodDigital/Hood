namespace Hood.Models
{
    public partial class ContentMeta : MetadataBase, IMetadata
    {
        public ContentMeta()
        {
        }

        public int ContentId { get; set; }
        public Content Content { get; set; }

    }
}

