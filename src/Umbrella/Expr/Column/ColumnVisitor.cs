using System.Linq.Expressions;

namespace Umbrella.Expr.Column
{
    internal class ColumnVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            switch ((DataTableExpression)node.NodeType)
            {
                case DataTableExpression.Column:
                    return VisitColumn((ColumnExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        protected virtual Expression VisitColumn(ColumnExpression c)
        {
            Expression columnMapper = Visit(c.ColumnMapper);
            if (c.ColumnMapper != columnMapper)
                return new ColumnExpression(columnMapper);

            return c;
        }
    }
}
