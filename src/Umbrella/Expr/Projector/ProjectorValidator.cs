using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projector
{
    internal static class ProjectorValidator
    {
        /// <summary>
        /// Checks if the projector's is a flat projector and the columns projected are primitives.
        /// </summary>
        /// <param name="projectorBody">Projector's content (body).</param>
        public static void Validate(Expression projectorBody, ParameterExpression projectorParameter)
        {
            var parameterReferencesFinder = new ParameterReferencesFinder();
            if (!parameterReferencesFinder.Find(projectorBody, projectorParameter))
                throw new InvalidOperationException("The projector's parameter is not being referenced in the projector's body.");

            if (!(projectorBody.NodeType == ExpressionType.New || projectorBody.NodeType == ExpressionType.MemberInit))
                throw new ArgumentException("The given projector should denote an object instantiation.");

            var nestedProjectionValidator = new NestedProjectionValidator();
            nestedProjectionValidator.Visit(projectorBody);
        }
    }
}
