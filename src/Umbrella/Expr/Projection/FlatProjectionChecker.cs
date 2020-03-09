using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    internal class FlatProjectionChecker: ExpressionVisitor
    {
        private NewExpression _newExpInScope = null;

        public bool IsFlatProjection { get; private set; }

        public void Validate(Expression expression)
        {
            try
            {
                _projection = expression;
                Visit(expression);

                if (_newExpInScope == null)
                    throw new InvalidProjectionException("The projection does not denote an object creation/instantiation.", expression);
            }
            finally
            {
                _projection = null;
                _newExpInScope = null;
            }
        }

        protected override Expression VisitNew(NewExpression ne)
        {
            if (_newExpInScope == null)
                _newExpInScope = ne;
            else
                throw new InvalidProjectionException("The projection is invalid because has nested projection(s).", _projection);

            return base.VisitNew(ne);
        }
    }
}
