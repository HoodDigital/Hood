using System.Linq;
using System.Threading.Tasks;
using Hood.Interfaces;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides an IEnumerable structure that can be used in paging applications.
    /// </summary>
    public interface IPagedList<T> : IPageableModel
    {
        string Order { get; set; }
        string Search { get; set; }
        List<T> List { get; set; }

        IPagedList<T> Reload(IPagedList<T> source);
        IPagedList<T> Reload(IEnumerable<T> source);
        IPagedList<T> Reload(IEnumerable<T> source, int pageIndex, int pageSize);
        Task<IPagedList<T>> ReloadAsync(IQueryable<T> source, int pageIndex, int pageSize);
        Task<IPagedList<T>> ReloadAsync(IQueryable<T> source);
    }

}
