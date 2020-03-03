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
using Umbrella.Xpression.Rewritters;
using Umbrella.Xpression.Projector;
using Umbrella.Xpression.Column;

namespace Umbrella
{
    internal class UmbrellaDataTable<T>
    {
        private readonly Expression _projector;
        private readonly IEnumerable<T> _source;

        private UmbrellaDataTable(IEnumerable<T> source, Expression projector)
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
            LambdaExpression projectorLambdaExp = (LambdaExpression)projector;

            ParameterExpression projectorParameter = projectorLambdaExp.Parameters[0];
            Expression projectorBody = projectorLambdaExp.Body;

            var parameterProjectedRewritter = new ParameterProjectedRewritter();
            projectorBody = parameterProjectedRewritter.Rewrite(projectorBody);

            var localEval = new ProjectorLocalEvaluator();
            projectorBody = localEval.Evaluate(projectorBody, projectorParameter);

            var columnSettingsRewritter = new ColumnSettingsRewritter();
            projectorBody = columnSettingsRewritter.Rewrite(projectorBody);

            ProjectorValidator.Validate(projectorBody, projectorParameter);

            var columnExpressionMapper = new ColumnExpressionMapper();
            projectorBody = columnExpressionMapper.Map(projectorBody);
            
            LambdaExpression newProjector = Expression.Lambda(projectorBody, projectorParameter);
            var umbrellaDataTable = new UmbrellaDataTable<TEntity>(source, newProjector);

            return umbrellaDataTable.GetDataTable();
        }

        private DataTable GetDataTable()
        {
            var columnsMapped = new ColumnsMapped(_projector);
            List<Column> columns = columnsMapped.GetColumns();

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
                    if (c.IsParameterless)
                        row[c.Name] = c.Mapper.DynamicInvoke();
                    else
                        row[c.Name] = c.Mapper.DynamicInvoke(data);

                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }


}
