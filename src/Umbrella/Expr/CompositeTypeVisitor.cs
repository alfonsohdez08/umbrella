using System.Linq.Expressions;

namespace Umbrella.Expr
{
    /// <summary>
    /// Composite type visitor.
    /// </summary>
    internal class CompositeTypeVisitor : ExpressionVisitor
    {
        private NewExpression _newExp = null;

        /// <summary>
        /// Denotes whether the projection is a composite type or not. A composite type is basically a structure that holds a set of members in it.
        /// </summary>
        public bool IsProjectingACompositeType => _newExp != null;

        protected override Expression VisitNew(NewExpression n)
        {
            _newExp = n;

            return n;
        }
    }

}