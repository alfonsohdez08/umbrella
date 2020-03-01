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
        public static void Validate(Expression projectorBody, ParameterExpression projectorParameter)
        {
            if (!(projectorBody.NodeType == ExpressionType.New || projectorBody.NodeType == ExpressionType.MemberInit))
                throw new ArgumentException("The given projector should denote an object instantiation.");

            var nestedObjectValidator = new ProjectorNestedObjectValidator();
            nestedObjectValidator.Visit(projectorBody);

            var projectionParameterVisitor = new ProjectionParameterVisitor(projectorParameter);
            projectionParameterVisitor.Visit(projectorBody);

            if (!projectionParameterVisitor.IsProjectorParameterReferenced)
                throw new InvalidOperationException("The projector's parameter is not being referenced in the projector's body.");
        }
    }

    public class ProjectionParameterVisitor: ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        private bool _isProjectorParameterReferenced = false;

        public bool IsProjectorParameterReferenced => _isProjectorParameterReferenced;

        public ProjectionParameterVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_parameter == p)
                _isProjectorParameterReferenced = true;

            return p;
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
