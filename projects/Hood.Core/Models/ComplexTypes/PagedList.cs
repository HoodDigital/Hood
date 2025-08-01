using Hood.Attributes;
using Hood.BaseTypes;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
            Reload(source, pageIndex, pageSize);
        }

        /// <summary>
        /// Page index
        /// </summary>
        [FromQuery(Name = "page")]
        [JsonProperty("page")]
        [Display(Name = "Current Page")]
        public virtual int PageIndex { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        [FromQuery(Name = "pageSize")]
        [JsonProperty("pageSize")]
        [Display(Name = "Page Size")]
        public virtual int PageSize { get; set; }

        /// <summary>
        /// Total count
        /// </summary>
        [RouteIgnore]
        [JsonProperty("totalCount")]
        [Display(Name = "Total Records", Description = "Total number of results returned from this set.")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Total pages
        /// </summary>
        [RouteIgnore]
        [JsonProperty("totalPages")]
        [Display(Name = "Total Pages", Description = "Total number of pages.")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Sorting Order - Used with <see cref="Hood.Interfaces.IPageableModel"/> sorting functions.
        /// </summary>
        [FromQuery(Name = "sort")]
        [JsonProperty("sort")]
        [Display(Name = "Sorting Order", Description = "The order for the results to be returned.")]
        public string Order { get; set; }
        /// <summary>
        /// Search filter - Used with <see cref="Hood.Interfaces.IPageableModel"/> sorting functions.
        /// </summary>
        [FromQuery(Name = "search")]
        [JsonProperty("search")]
        [Display(Name = "Search", Description = "The search term to filter the results by.")]
        public string Search { get; set; }

        /// <summary>
        /// Has previous page
        /// </summary>
        [RouteIgnore]
        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage => (PageIndex > 1);
        /// <summary>
        /// Has next page
        /// </summary>
        [RouteIgnore]
        [JsonProperty("hasNextPage")]
        public bool HasNextPage => (PageIndex < TotalPages);


        public List<T> List
        {
            set => Reload(value, PageIndex, PageSize);
            get => _list;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            Reload(source, pageIndex, pageSize);
        }

        public IPagedList<T> Reload(IPagedList<T> source)
        {
            Reload(source.List, source.PageIndex, source.PageSize);
            return this;
        }

        public IPagedList<T> Reload(IEnumerable<T> source)
        {
            Reload(source, PageIndex, PageSize);
            return this;
        }

        public IPagedList<T> Reload(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            int total = source.Count();
            TotalCount = total;
            TotalPages = (int)Math.Ceiling((double)total / pageSize);
            if (pageIndex > TotalPages)
            {
                pageIndex = TotalPages;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            _list = new List<T>();
            if (PageSize > TotalCount)
            {
                _list.AddRange(source.ToList());
            }
            else
            {
                _list.AddRange(source.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList());
            }

            return this;
        }

        public async Threading.Tasks.Task<IPagedList<T>> ReloadAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            int total = await source.CountAsync();
            TotalCount = total;
            TotalPages = (int)Math.Ceiling((double)total / pageSize);
            if (pageIndex > TotalPages)
            {
                pageIndex = TotalPages;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            _list = new List<T>();
            if (PageSize > TotalCount)
            {
                _list.AddRange(await source.ToListAsync());
            }
            else
            {
                _list.AddRange(await source.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync());
            }

            return this;
        }

        public async Threading.Tasks.Task<IPagedList<T>> ReloadAsync(IQueryable<T> source)
        {
            int total = await source.CountAsync();
            TotalCount = total;
            TotalPages = (int)Math.Ceiling((double)total / PageSize);
            if (PageIndex > TotalPages)
            {
                PageIndex = TotalPages;
            }

            _list = new List<T>();
            if (PageSize > TotalCount)
            {
                _list.AddRange(await source.ToListAsync());
            }
            else
            {
                _list.AddRange(await source.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync());
            }

            return this;
        }

        public virtual string GetPageUrl(int pageIndex)
        {
            string query = "?";
            if (pageIndex > 0)
            {
                query = $"?page={pageIndex}&pageSize={PageSize}";
            }
            query += Search.IsSet() ? "&search=" + System.Net.WebUtility.UrlEncode(Search) : "";
            query += Order.IsSet() ? "&sort=" + System.Net.WebUtility.UrlEncode(Order) : "";
            return query;
        }
    }
}