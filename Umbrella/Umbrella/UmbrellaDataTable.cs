using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;
using System.Linq;

namespace Umbrella
{
    internal class UmbrellaDataTable<T>
    {
        private readonly Expression _expression;
        private readonly List<T> _list;
        private readonly Dictionary<DataColumn, PropertyInfo> _columns = new Dictionary<DataColumn, PropertyInfo>();

        private UmbrellaDataTable(List<T> list, Expression expression)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            _list = list;
            _expression = expression;

        }

        public static DataTable Build<TEntity>(List<TEntity> list, Expression projector)
        {
            var umbrellaDataTable = new UmbrellaDataTable<TEntity>(list, projector);

            return umbrellaDataTable.GetDataTable();
        }

        private DataTable GetDataTable()
        {
            Dictionary<DataColumn, Delegate> bindings = DataColumnBinding.GetColumnsBinded(_expression);

            var dataTable = new DataTable();
            foreach (DataColumn c in bindings.Keys)
                dataTable.Columns.Add(c);

            foreach (T data in _list)
            {
                DataRow row = dataTable.NewRow();
                foreach (var b in bindings)
                    row[b.Key] = b.Value.DynamicInvoke(data);
            }

            return dataTable;
        }
    }
}
