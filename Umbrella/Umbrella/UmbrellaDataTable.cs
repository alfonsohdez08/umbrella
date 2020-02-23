using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Umbrella
{
    internal class UmbrellaDataTable<T>
    {
        private readonly Expression _expression;
        private readonly IEnumerable<T> _source;

        private UmbrellaDataTable(IEnumerable<T> source, Expression expression)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            _source = source;
            _expression = expression;

        }

        /// <summary>
        /// Constructs a DataTable with the <paramref name="source"/>'s data and the columns encountered in the <paramref name="projector"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type that contains the collection.</typeparam>
        /// <param name="source">Source collection.</param>
        /// <param name="projector">Columns projector.</param>
        /// <returns>A filled DataTable.</returns>
        public static DataTable Build<TEntity>(IEnumerable<TEntity> source, Expression projector)
        {
            var umbrellaDataTable = new UmbrellaDataTable<TEntity>(source, projector);

            return umbrellaDataTable.GetDataTable();
        }

        private DataTable GetDataTable()
        {
            Dictionary<DataColumn, Delegate> bindings = ColumnsMapping.GetColumns(_expression);

            var dataTable = new DataTable();
            foreach (DataColumn c in bindings.Keys)
                dataTable.Columns.Add(c);

            foreach (T data in _source)
            {
                DataRow row = dataTable.NewRow();
                foreach (var b in bindings)
                    row[b.Key] = b.Value.DynamicInvoke(data);

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
