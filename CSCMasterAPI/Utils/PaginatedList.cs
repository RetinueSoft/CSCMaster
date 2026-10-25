using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSCMasterAPI.Util
{
    public class PaginatedList<T>
    {
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public List<T> PagedList { get; private set; }
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PagedList = new List<T>();
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PagedList.AddRange(items);
        }
        public bool PreviousPage
        {
            get { return PageIndex > 1; }
        }

        public bool NextPage
        {
            get { return PageIndex < TotalPages; }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
        public PaginatedList<T1> MapTo<T1>(Func<T, T1> mapper)
        {
            var mappedItems = PagedList.Select(mapper).ToList();

            return new PaginatedList<T1>(mappedItems, TotalCount, PageIndex, PageSize);
        }
    }
}
