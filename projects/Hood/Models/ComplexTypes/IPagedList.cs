namespace System.Collections.Generic
{
    /// <summary>
    /// Provides an IEnumerable structure that can be used in paging applications.
    /// </summary>
    public interface IPagedList<T> 
    {
        List<T> List { get; set; }
        /// <summary>
        /// Page index
        /// </summary>
        int PageIndex { get; }
        /// <summary>
        /// Page size
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// Total count
        /// </summary>
        int TotalCount { get; }
        /// <summary>
        /// Total pages
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// Has previous page
        /// </summary>
        bool HasPreviousPage { get; }
        /// <summary>
        /// Has next age
        /// </summary>
        bool HasNextPage { get; }
    }

}
