using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Rewritters
{
    /// <summary>
    /// Base expression rewritter.
    /// </summary>
    internal abstract class ExpressionRewritter: ExpressionVisitor
    {
        /// <summary>
        /// Rewrites an expression of form A to another one of form B.
        /// </summary>
        /// <param name="expression">Expression that would be rewritten.</param>
        /// <returns>A modified expression if found any rewritable subtree; otherwise the same expression.</returns>
        public abstract Expression Rewrite(Expression expression);
    }
}
