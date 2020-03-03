using System.Linq.Expressions;

namespace Umbrella.Xpression.Column
{
    public class ColumnVisitor : ExpressionVisitor
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
            Expression columnDefinition = Visit(c.ColumnDefinition);
            if (c.ColumnDefinition != columnDefinition)
                return new ColumnExpression(columnDefinition);

            return c;
        }
    }
}
