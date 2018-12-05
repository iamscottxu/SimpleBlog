using System;

namespace Scottxu.Blog.Models.ViewModel
{
    public class PageInfoViewModel
    {
        int _recordCount;
        int _pageIndex;

        public int RecordCount
        {
            get => _recordCount;
            set
            {
                _recordCount = value;
                PageIndex = PageIndex > PageCount - 1 ? PageCount - 1 : PageIndex;
            }
        }

        public int PageCount
        {
            get
            {
                if (RecordCount == 0) return 1;
                var pageCount = RecordCount / PageSize;
                if (RecordCount % PageSize != 0) pageCount++;
                return pageCount;
            }
        }

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value > 0 ? value : 0;
        }

        public int PageSize { get; set; }

        public string SortField { get; set; }

        public string SortDirection { get; set; }
    }
}