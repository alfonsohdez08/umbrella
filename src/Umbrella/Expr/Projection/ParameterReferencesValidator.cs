using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Projection
{
    internal class ParameterReferencesValidator : ExpressionVisitor, IExpressionValidator
    {
        public void Validate(Expression expression)
        {
            var projector = (LambdaExpression)expression;

            var paramSeeker = new ParameterSeeker();
            if (!paramSeeker.Exists(projector.Body, projector.Parameters[0]))
                throw new InvalidProjectionException("The projection does not hold any reference to the projector's parameter.", projector.Body);
        }
    }
}
