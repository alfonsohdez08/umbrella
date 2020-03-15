using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Projection
{
    /// <summary>
    /// Validator for flat projections.
    /// </summary>
    internal class FlatProjectionValidator : ExpressionVisitor, IExpressionValidator
    {
        private NewExpression _newExpInScope = null;
        private Expression _projection;

        /// <summary>
        /// Checks if the projection does not have nested projection (meaning nested new operators).
        /// </summary>
        /// <param name="expression">Projector.</param>
        public void Validate(Expression expression)
        {
            try
            {
                _projection = expression;
                Visit(expression);

                //if (_newExpInScope == null)
                //    throw new InvalidProjectionException("The projection does not denote an object construction.", expression);
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
