namespace admin_sweetsoft_tech_support.Attributes
{
    public class TableSorter
    {
        public static List<T> Sort<T>(List<T> items, string sortColumn, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortColumn))
            {
                return items;
            }

            // Sử dụng Reflection để lấy giá trị của thuộc tính dựa trên sortColumn
            var property = typeof(T).GetProperty(sortColumn);
            if (property == null)
            {
                throw new ArgumentException($"Property '{sortColumn}' not found on type '{typeof(T).Name}'");
            }

            return sortOrder == "desc"
                ? items.OrderByDescending(item => property.GetValue(item, null)).ToList()
                : items.OrderBy(item => property.GetValue(item, null)).ToList();
        }
    }
}
