using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Xpression.Nominators;

namespace Umbrella.Xpression.Column
{
    public class ColumnExpressionMapper: ExpressionVisitor
    {
        private HashSet<Expression> _nominatedColumns = new HashSet<Expression>();

        public Expression Map(Expression projectorBody)
        {
            var columnsNominator = new ColumnsNominator();
            _nominatedColumns =  columnsNominator.Nominate(projectorBody);

            try
            {
                return Visit(projectorBody);
            }
            finally
            {
                _nominatedColumns = null;
            }
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
