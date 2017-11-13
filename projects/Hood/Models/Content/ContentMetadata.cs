using Hood.Entities;
using Hood.Interfaces;

namespace Hood.Models
{
    public partial class ContentMeta : BaseEntity, IMetadata
    {
        public string BaseValue { get; set; }
        public bool IsStored { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int ContentId { get; set; }
        public Content Content { get; set; }
    }
}

