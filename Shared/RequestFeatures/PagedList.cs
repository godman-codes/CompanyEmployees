using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.RequestFeatures
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            // Create a new instance of MetaData and set its properties
            MetaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            // Add the items to the list
            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            // Count the total number of items
            var count = source.Count();

            // Retrieve the items for the specified page
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create a new instance of PagedList and return it
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
