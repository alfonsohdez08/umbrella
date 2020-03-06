using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    internal class ProjectionValidator : IExpressionValidator
    {
        /// <summary>
        /// Validates that the projection is valid. A projection is valid if it's a flat projection and references at least one time the projector's parameter.
        /// </summary>
        /// <param name="expression">The projector.</param>
        public void Validate(Expression expression)
        {
            var paramProjectionValidator = new ParameterReferencesValidator();
            paramProjectionValidator.Validate(expression);

            var flatProjectionValidator = new FlatProjectionValidator();
            flatProjectionValidator.Validate(expression);
        }
    }
}
