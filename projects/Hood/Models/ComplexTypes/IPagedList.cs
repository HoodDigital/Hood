using Hood.Interfaces;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides an IEnumerable structure that can be used in paging applications.
    /// </summary>
    public interface IPagedList<T> : IPageableModel
    {
        List<T> List { get; set; }
    }

}
