using System.Collections.Generic;
using System.Linq.Expressions;
using Umbrella.Expr.Column;

namespace Umbrella.Expr.UnitTests
{
    internal class ColumnExpressionsFetcher: ExpressionVisitor
    {
        private List<ColumnExpression> _columns = new List<ColumnExpression>();

        public List<ColumnExpression> FetchAll(Expression projection)
        {
            Visit(projection);

            return _columns;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                switch ((DataTableExpression)node.NodeType)
                {
                    case DataTableExpression.Column:
                        return VisitColumn((ColumnExpression)node);
                }
            }

            return base.Visit(node);
        }

        protected virtual Expression VisitColumn(ColumnExpression c)
        {
            _columns.Add(c);

            return c;
        }
    }
}
