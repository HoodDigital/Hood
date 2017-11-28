using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            this.Reload(source, pageIndex, pageSize);
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            this.Reload(source, pageIndex, pageSize);
        }

        /// <summary>
        /// Sets the current IEnumerable with the items passed in as the value, using the pagesize and index of the current list.
        /// </summary>
        public IEnumerable<T> Items
        {
            set
            {
                this.Reload(value, this.PageIndex, this.PageSize);
            }
        }

        private void Reload(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var total = source.Count();
            this.TotalCount = total;
            this.TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Page index
        /// </summary>
        [FromQuery(Name = "page")]
        public int PageIndex { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; }
        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; private set; }
        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; private set; }
        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }
        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage
        {
            get { return (PageIndex + 1 < TotalPages); }
        }
    }
}