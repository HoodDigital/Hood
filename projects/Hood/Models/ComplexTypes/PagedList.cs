using Hood.BaseTypes;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public partial class PagedList<T> : SaveableModel, IPagedList<T>
    {
        private List<T> _list;
        /// <summary>
        /// Constructor
        /// </summary>
        public PagedList()
        {
            PageSize = 20;
            PageIndex = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            this.Reload(source, pageIndex, pageSize);
        }


        /// <summary>
        /// Page index
        /// </summary>
        [FromQuery(Name = "page")]
        [Display(Name = "Current Page")]
        public int PageIndex { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        [FromQuery(Name = "pageSize")]
        [Display(Name = "Page Size")]
        public int PageSize { get; set; }
        /// <summary>
        /// Total count
        /// </summary>
        [Display(Name = "Total Records", Description = "Total number of results returned from this set.")]
        public int TotalCount { get; private set; }
        /// <summary>
        /// Total pages
        /// </summary>
        [Display(Name = "Total Pages", Description = "Total number of pages.")]
        public int TotalPages { get; private set; }

        /// <summary>
        /// Sorting Order - Used with <see cref="Hood.Interfaces.IPageableModel"/> sorting functions.
        /// </summary>
        [FromQuery(Name = "sort")]
        [Display(Name = "Sorting Order", Description = "The order for the results to be returned.")]
        public string Order { get; set; }
        /// <summary>
        /// Search filter - Used with <see cref="Hood.Interfaces.IPageableModel"/> sorting functions.
        /// </summary>
        [FromQuery(Name = "search")]
        [Display(Name = "Search", Description = "The search term to filter the results by.")]
        public string Search { get; set; }

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


        public List<T> List
        {
            set
            {
                Reload(value, this.PageIndex, this.PageSize);
            }
            get
            {
                return _list;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            this.Reload(source, pageIndex, pageSize);
        }

        public IPagedList<T> Reload(IPagedList<T> source)
        {
            Reload(source.List, source.PageIndex, source.PageSize);
            return this;
        }

        public IPagedList<T> Reload(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var total = source.Count();
            this.TotalCount = total;
            this.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            if (pageIndex > TotalPages)
                pageIndex = TotalPages;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            _list = new List<T>();
            _list.AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
            return this;
        }

        public async Threading.Tasks.Task<IPagedList<T>> ReloadAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var total = await source.CountAsync();
            this.TotalCount = total;
            this.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            if (pageIndex > TotalPages)
                pageIndex = TotalPages;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            _list = new List<T>();
            _list.AddRange(await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
            return this;
        }

        public async Threading.Tasks.Task<IPagedList<T>> ReloadAsync(IQueryable<T> source)
        {
            var total = await source.CountAsync();
            this.TotalCount = total;
            this.TotalPages = (int)Math.Ceiling((double)total / PageSize);
            if (PageIndex > TotalPages)
                PageIndex = TotalPages;
            _list = new List<T>();
            _list.AddRange(await source.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync());
            return this;
        }

        public virtual string GetPageUrl(int pageIndex)
        {
            var query = $"?page={pageIndex}&pageSize={PageSize}";
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }
    }
}