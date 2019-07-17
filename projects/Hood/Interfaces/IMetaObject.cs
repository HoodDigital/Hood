using Hood.Models;
using System.Collections.Generic;

namespace Hood.Interfaces
{
    public interface IMetaObect<TMetadata>
        where TMetadata : IMetadata
    {
        List<TMetadata> Metadata { get; set; }
    }

}
