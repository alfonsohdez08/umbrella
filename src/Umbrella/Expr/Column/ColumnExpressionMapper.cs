using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Nominators;

namespace Umbrella.Expr.Column
{
    /// <summary>
    /// Acts as DataTable column expression mapper.
    /// </summary>
    internal class ColumnExpressionMapper: ExpressionVisitor
    {
        private HashSet<Expression> _nominatedColumns = new HashSet<Expression>();

        /// <summary>
        /// Wraps all subtrees that can be a column by ColumnExpression instances.
        /// </summary>
        /// <param name="lambda">Projector.</param>
        /// <returns>An expression that has column expression on it (a column expression means a DataTable's column).</returns>
        public Expression Map(LambdaExpression lambda)
        {
            Expression projectionMapped = null;

            try
            {
                var columnsNominator = new ColumnsNominator();
                _nominatedColumns = columnsNominator.Nominate(lambda.Body);

                projectionMapped = Visit(lambda.Body);
            }
            finally
            {
                _nominatedColumns = null;
            }

            return Expression.Lambda(projectionMapped, lambda.Parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_nominatedColumns.Contains(node))
            {
                var columnExpression = new ColumnExpression(node);

                return columnExpression;
            }

            return base.Visit(node);
        }
    }
}
