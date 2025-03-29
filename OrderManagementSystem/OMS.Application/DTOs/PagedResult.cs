using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Application.DTOs
{
    public class PagedResult<T>
    {
        
        
        
        public IEnumerable<T> Items { get; set; } = new List<T>();

        
        
        
        public int TotalCount { get; set; }

        
        
        
        public int Page { get; set; }

        
        
        
        public int PageSize { get; set; }

        
        
        
        public int TotalPages { get; set; }

        
        
        
        public bool HasPreviousPage => Page > 1;

        
        
        
        public bool HasNextPage => Page < TotalPages;

        
        
        
        public bool HasItems => Items != null && Items.Any();

        
        
        
        public int FirstItemIndex => (Page - 1) * PageSize + 1;

        
        
        
        public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);

        
        
        
        public IEnumerable<int> PageNumbers
        {
            get
            {
                const int maxPageNumbers = 5;
                int startPage = Math.Max(1, Page - maxPageNumbers / 2);
                int endPage = Math.Min(TotalPages, startPage + maxPageNumbers - 1);

                if (endPage - startPage + 1 < maxPageNumbers)
                {
                    startPage = Math.Max(1, endPage - maxPageNumbers + 1);
                }

                return Enumerable.Range(startPage, Math.Min(maxPageNumbers, endPage - startPage + 1));
            }
        }
    }
}
