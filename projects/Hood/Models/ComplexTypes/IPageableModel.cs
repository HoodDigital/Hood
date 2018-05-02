namespace Hood.Interfaces
{
    public interface IPageableModel
    {
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

        /// <summary>
        /// Querystring for pagers.
        /// </summary>
        string GetPageUrl(int pageIndex);
    }
}