using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbrella.Expr.Nominators
{
    /// <summary>
    /// Base nominator.
    /// </summary>
    internal abstract class Nominator: ExpressionVisitor
    {
        /// <summary>
        /// Nominates expression's nodes based on the implementer rules.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <returns>A set of nodes that follow the implementer rules.</returns>
        public abstract HashSet<Expression> Nominate(Expression expression);
    }
}