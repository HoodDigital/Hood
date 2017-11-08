using System.Collections.Generic;

namespace Hood.Interfaces
{
    public interface ICategory<TContent>
        where TContent : IContent<IMetadata, IMediaObject, IHoodUser>
    {
        List<ICategory<TContent>> Children { get; set; }
        string DisplayName { get; set; }
        ICategory<TContent> ParentCategory { get; set; }
        int? ParentCategoryId { get; set; }
    }
}