using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Nominators;

namespace Umbrella.Expr.Column
{
    internal class ColumnExpressionMapper: ExpressionVisitor
    {
        private HashSet<Expression> _nominatedColumns = new HashSet<Expression>();

        /// <summary>
        /// Maps property's binding expression to a column expression.
        /// </summary>
        /// <param name="projectorBody">A flat projector (meaning, no nested projections).</param>
        /// <returns>A projector where each property's binding expression has been replaced by a column expression.</returns>
        public Expression Map(Expression projectorBody)
        {
            var columnsNominator = new ColumnsNominator();
            _nominatedColumns =  columnsNominator.Nominate(projectorBody);

            Expression projectorWithColumnsMapped = null;
            try
            {
                projectorWithColumnsMapped = Visit(projectorBody);
            }
            finally
            {
                _nominatedColumns = null;
            }

            return projectorWithColumnsMapped;
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
