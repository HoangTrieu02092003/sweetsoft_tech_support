using System.Linq.Dynamic.Core;
namespace admin_sweetsoft_tech_support.Attributes
{
    public class TableSorter
    {
        public static IQueryable<T> Sort<T>(IQueryable<T> items, string sortColumn, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortColumn))
            {
                return items;
            }

            string sortingExpression = $"{sortColumn} {(sortOrder == "desc" ? "descending" : "ascending")}";
            return items.OrderBy(sortingExpression);
        }
    }

}
