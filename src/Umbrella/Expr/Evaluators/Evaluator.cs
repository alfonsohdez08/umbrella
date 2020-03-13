using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Evaluators
{
    /// <summary>
    /// Base evaluator.
    /// </summary>
    internal abstract class Evaluator: ExpressionVisitor
    {
        /// <summary>
        /// Evaluates a subtree based on implementer rules.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <returns>A modified expression if found any evaluable subtree; otherwise the same expression.</returns>
        public abstract Expression Evaluate(LambdaExpression expression);
    }
}
