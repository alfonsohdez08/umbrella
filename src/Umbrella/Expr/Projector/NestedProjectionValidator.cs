using System;
using System.Linq.Expressions;

namespace Umbrella.Expr.Projector
{
    internal class NestedProjectionValidator: ExpressionVisitor
    {
        private NewExpression _newExpInScope = null;

        public void Validate(Expression expression)
        {
            Visit(expression);
        }

        protected override Expression VisitNew(NewExpression ne)
        {
            if (_newExpInScope == null)
                _newExpInScope = ne;
            else
                throw new InvalidOperationException("The given projector has nested object instantiation.");

            return base.VisitNew(ne);
        }
    }
}
