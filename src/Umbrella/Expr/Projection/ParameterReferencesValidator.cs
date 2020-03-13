using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Projection
{
    /// <summary>
    /// Validator for parameter references.
    /// </summary>
    internal class ParameterReferencesValidator : ExpressionVisitor, IExpressionValidator
    {
        /// <summary>
        /// Checks whether the projection references the projector's parameter.
        /// </summary>
        /// <param name="expression">Projector.</param>
        public void Validate(Expression expression)
        {
            var projector = (LambdaExpression)expression;

            var paramSeeker = new ParameterSeeker();
            if (!paramSeeker.Exists(projector.Body, projector.Parameters[0]))
                throw new InvalidProjectionException("The projection does not hold any reference to the projector's parameter.", projector.Body);
        }
    }
}
