using System;
using System.Linq.Expressions;

namespace Umbrella.Expr.Projector
{
    public class NestedProjectionValidator: ExpressionVisitor
    {
        private NewExpression _newExpInScope = null;

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
