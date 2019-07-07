using System.Collections.Generic;

namespace Hood.Models
{
    public interface IJsonMetadata
    {
        Dictionary<string, string> Metadata { get; set; }
    }
}