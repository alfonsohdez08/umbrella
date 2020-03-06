using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Umbrella.Tests
{
    public static class DataTableExtensions
    {
        public static bool HasColumns(this System.Data.DataTable dataTable, params string[] columns)
        {
            for (int index = 0; index < columns.Length; index++)
            {
                string columnName = columns[index];

                if (!dataTable.Columns.Contains(columnName))
                    return false;
            }

            return true;
        }

        public static List<DataColumn> ToList(this DataColumnCollection columnCollection)
        {
            DataColumn[] columns = new DataColumn[columnCollection.Count];

            columnCollection.CopyTo(columns, 0);

            return columns.ToList();
        }

        internal static bool HasAllColumns(this List<Column> columns, params string[] columnNames)
        {
            return columns.TrueForAll(c => columnNames.Contains(c.Name));
        }
    }
}
