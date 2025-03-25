using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Application.DTOs
{
    public class PagedResult<T>
    {
        /// <summary>
        /// Mevcut sayfadaki öğeler
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Toplam öğe sayısı
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Mevcut sayfa numarası (1 tabanlı)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Sayfa başına gösterilecek öğe sayısı
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Toplam sayfa sayısı
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Önceki sayfa var mı?
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Sonraki sayfa var mı?
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Mevcut sayfada öğe var mı?
        /// </summary>
        public bool HasItems => Items != null && Items.Any();

        /// <summary>
        /// Bu sayfanın ilk öğesinin indeksi
        /// </summary>
        public int FirstItemIndex => (Page - 1) * PageSize + 1;

        /// <summary>
        /// Bu sayfanın son öğesinin indeksi
        /// </summary>
        public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);

        /// <summary>
        /// Sayfalar için kullanılacak navigasyon bağlantı sayısı
        /// </summary>
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
