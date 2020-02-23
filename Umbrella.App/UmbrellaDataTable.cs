using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.App.Exceptions;
using System.Linq;

namespace Umbrella.App
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
            ColumnCandidates cc = ProjectionVisitor.GetCandidates(_expression);
            
            LambdaExpression le = (LambdaExpression)_expression;
            Type type = le.Body.Type;

            Dictionary<DataColumn, Delegate> bindings = ColumnBindingsVisitor.GetColumnBindings(le.Body, type.GetProperties().ToList(), cc);

            var dataTable = new DataTable();
            foreach (DataColumn dc in bindings.Keys)
            {
                dataTable.Columns.Add(dc);
            }

            foreach (T data in _list)
            {
                DataRow row = dataTable.NewRow();
                foreach (var dcb in bindings)
                    row[dcb.Key] = dcb.Value.DynamicInvoke(data);

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }

}
