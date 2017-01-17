using System.Collections.Generic;
using System.Linq;

namespace Hood.Interfaces
{
    public interface IMetaObect<TMetadata>
        where TMetadata : IMetadata
    {
        List<TMetadata> Metadata { get; set; }
    }

    public static class IMetaObjectExtensions
    {
        public static TMetadata GetMeta<TMetadata>(this IMetaObect<TMetadata> from, string name)
            where TMetadata : IMetadata, new()
        {
            TMetadata cm = from.Metadata.FirstOrDefault(p => p.Name == name);
            if (cm == null)
                return new TMetadata()
                {
                    BaseValue = null,
                    Name = name,
                    Type = null
                };
            return cm;
        }

        public static void UpdateMeta<T, TMetadata>(this IMetaObect<TMetadata> from, string name, T value)
            where TMetadata : IMetadata, new()
        {
            if (from.Metadata != null)
            {
                TMetadata cm = from.Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    cm.Set(value);
                }
            }
        }

        public static void AddMeta<TMetadata>(this IMetaObect<TMetadata> from, TMetadata value)
            where TMetadata : IMetadata, new()
        {
            if (from.Metadata != null)
            {
                from.Metadata.Add(value);
            }
        }

        public static bool HasMeta<TMetadata>(this IMetaObect<TMetadata> from, string name)
            where TMetadata : IMetadata, new()
        {
            if (from.Metadata != null)
            {
                TMetadata cm = from.Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                    return true;
            }
            return false;
        }

    }

}
