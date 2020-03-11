using System.Linq.Expressions;

namespace Umbrella.Expr
{
    internal class CompositeTypeVisitor : ExpressionVisitor
    {
        private NewExpression _newExp = null;

        /// <summary>
        /// Denotes whether the projection is a complex type or not. A complex type is basically a structure that has members.
        /// </summary>
        public bool IsProjectingACompositeType => _newExp != null;

        protected override Expression VisitNew(NewExpression n)
        {
            _newExp = n;

            return n;
        }
    }

}