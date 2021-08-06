using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace pitermarx.Data
{
    public interface IDataTableable
    {
        DataColumn[] GetSchema();

        object[] GetRow();
    }

    public static class DataTableUtils
    {
        public static DataTable ToDataTable<T>(this List<T> list) where T : IDataTableable
        {
            var table = new DataTable();
            if (!list.Any()) return table;

            table.Columns.AddRange(list.First().GetSchema());

            foreach (var item in list)
            {
                table.Rows.Add(item.GetRow());
            }

            return table;
        }
    }
}