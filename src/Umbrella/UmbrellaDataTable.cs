using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using Umbrella.Extensions;
using Umbrella.Expr.Rewritters;
using Umbrella.Expr.Projection;
using Umbrella.Expr.Column;

namespace Umbrella
{
    internal class UmbrellaDataTable<T>
    {
        private readonly LambdaExpression _projector;
        private readonly IEnumerable<T> _source;

        private UmbrellaDataTable(IEnumerable<T> source, LambdaExpression projector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            _source = source;
            _projector = projector;
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
            var umbrellaDataTable = new UmbrellaDataTable<TEntity>(source, (LambdaExpression)projector);

            return umbrellaDataTable.GetDataTable();
        }

        private DataTable GetDataTable()
        {
            var columnsMapping = new ColumnsMapping(_projector);
            List<Column> columns = columnsMapping.GetColumns();

            var dataTable = new DataTable();
            foreach (Column c in columns)
            {
                var dataColumn = new DataColumn(c.Name, c.DataType);
                dataColumn.AllowDBNull = c.IsNullable;

                dataTable.Columns.Add(dataColumn);
            }

            foreach (T data in _source)
            {
                DataRow row = dataTable.NewRow();
                foreach (Column c in columns)
                {
                    object val = null;
                    if (c.IsMapperParameterless)
                        val = c.Mapper.DynamicInvoke();
                    else
                        val = c.Mapper.DynamicInvoke(data);

                    if (c.IsNullable && val == null)
                        val = DBNull.Value;

                    row[c.Name] = val;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }


}
