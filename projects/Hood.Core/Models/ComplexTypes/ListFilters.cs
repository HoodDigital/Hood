namespace Hood.Models
{
    public class ListFilters
    {
        public int skip { get; set; }
        private int _take;
        public int take
        {
            get
            {
                if (_take == 0)
                    return 12;
                return _take;
            }
            set { _take = value; }
        }
        private int _page;
        public int page
        {
            get
            {
                if (_page == 0)
                    return 1;
                return _page;
            }
            set { _page = value; }
        }
        private int _pageSize;
        public int pageSize
        {
            get
            {
                if (_pageSize == 0)
                    return 12;
                return _pageSize;
            }
            set { _pageSize = value; }
        }
        public string search { get; set; }
        public string sort { get; set; }
    }
}
