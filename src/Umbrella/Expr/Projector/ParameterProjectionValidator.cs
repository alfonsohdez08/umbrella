using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Projector
{
    internal class ParameterProjectionValidator : ExpressionVisitor, IExpressionValidator
    {
        public void Validate(Expression expression)
        {
            var projector = (LambdaExpression)expression;

            var paramFinder = new ParameterReferencesFinder();
            if (!paramFinder.Find(projector.Body, projector.Parameters[0]))
                throw new InvalidProjectionException("The projection does not hold any reference to the projector's parameter", projector.Body);
        }
    }
}
