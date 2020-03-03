using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Nominators;

namespace Umbrella.Expr.Column
{
    public class ColumnExpressionMapper: ExpressionVisitor
    {
        private HashSet<Expression> _nominatedColumns = new HashSet<Expression>();

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
