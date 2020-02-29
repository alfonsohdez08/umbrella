using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella
{
    public class ProjectorValidator: ExpressionVisitor
    {
        /// <summary>
        /// Checks if the projector's is a flat projector and the columns projected are primitives.
        /// </summary>
        /// <param name="projectorBody">Projector's content (body).</param>
        public static void Validate(Expression projectorBody)
        {
            if (!(projectorBody.NodeType == ExpressionType.New || projectorBody.NodeType == ExpressionType.MemberInit))
                throw new ArgumentException("The given projector should denote an object instantiation.");

            var nestedObjectValidator = new ProjectorNestedObjectValidator();
            nestedObjectValidator.Visit(projectorBody);
        }
    }

    public class ProjectorNestedObjectValidator: ExpressionVisitor
    {
        private NewExpression _newExpInScope = null;

        protected override Expression VisitNew(NewExpression ne)
        {
            if (_newExpInScope == null)
                _newExpInScope = ne;
            else
                throw new InvalidOperationException("The given projector has nested object instantiation.");

            return base.VisitNew(ne);
        }
    }
}
