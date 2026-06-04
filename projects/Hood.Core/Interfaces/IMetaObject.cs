using System.Collections.Generic;
using Hood.Models;

namespace Hood.Interfaces
{
    public interface IMetaObect<TMetadata>
        where TMetadata : IMetadata
    {
        List<TMetadata> Metadata { get; set; }
    }
}
