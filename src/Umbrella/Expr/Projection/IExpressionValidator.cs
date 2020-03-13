using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    /// <summary>
    /// Defines an expression validator.
    /// </summary>
    internal interface IExpressionValidator
    {
        /// <summary>
        /// Validates an expression based on a set of rules defined by the implementer.
        /// </summary>
        /// <param name="expression">Expression.</param>
        void Validate(Expression expression);
    }
}