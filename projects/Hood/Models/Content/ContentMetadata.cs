using Hood.Interfaces;

namespace Hood.Models
{
    public partial class ContentMeta : IMetadata
    {
        public int Id { get; set; }
        public string BaseValue { get; set; }
        public bool IsStored { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }

    }

}

