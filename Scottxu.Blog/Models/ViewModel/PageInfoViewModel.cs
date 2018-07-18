using System;
namespace Scottxu.Blog.Models.ViewModel
{
    public class PageInfoViewModel
    {
        int recordCount;
        int pageIndex;

        public int RecordCount { 
            get => recordCount; 
            set {
                recordCount = value;
                PageIndex = PageIndex > PageCount - 1 ? PageCount - 1 : PageIndex;
            }
        }

        public int PageCount {
            get {
                if (RecordCount == 0) return 1;
                int pageCount = RecordCount / PageSize;
                if (RecordCount % PageSize != 0) pageCount++;
                return pageCount;
            }
        }

        public int PageIndex { 
            get => pageIndex; 
            set => pageIndex = value > 0 ? value : 0;
        }

        public int PageSize { get; set; }

        public string SortField { get; set; }

        public string SortDirection { get; set; }
    }
}
