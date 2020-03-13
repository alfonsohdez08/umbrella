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
    /// <summary>
    /// Wrapper around the System.Data.DataTable.
    /// </summary>
    /// <typeparam name="T">The type of the input dataset.</typeparam>
    internal class UmbrellaDataTable<T>
    {
        /// <summary>
        /// Holds the projection of a structure that's inspected in order to infer/get the columns along with its mapping of the DataTable.
        /// </summary>
        private readonly LambdaExpression _projector;

        /// <summary>
        /// The dataset that would be dumped into the DataTable.
        /// </summary>
        private readonly IEnumerable<T> _source;

        /// <summary>
        /// Creates an instance of an <c>UmbrellaDataTable</c>.
        /// </summary>
        /// <param name="source">Input dataset.</param>
        /// <param name="projector">Columns projector.</param>
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
        /// Builds a DataTable based on a projection.
        /// </summary>
        /// <typeparam name="TEntity">Type that holds the set (the type parameter of an IEnumerable<>).</typeparam>
        /// <param name="source">Dataset.</param>
        /// <param name="projector">Projects a structure that defines the columns of the DataTable and how the data would be mapped into it (mapping expressions).</param>
        /// <returns>A filled DataTable where its columns were inferred/discovered by the projection.</returns>
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
