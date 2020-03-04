using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projector
{
    internal static class ProjectorValidator
    {
        /// <summary>
        /// Checks if the projector's is a flat one and the columns projected are primitives.
        /// </summary>
        /// <param name="projector">Projector.</param>
        public static void Validate(LambdaExpression projector)
        {
            var parameterReferencesFinder = new ParameterReferencesFinder();
            if (!parameterReferencesFinder.Find(projector.Body, projector.Parameters[0]))
                throw new InvalidOperationException("The projector's parameter is not being referenced in the projector's body.");

            if (!(projector.Body.NodeType == ExpressionType.New || projector.Body.NodeType == ExpressionType.MemberInit))
                throw new ArgumentException("The given projector should denote an object instantiation.");

            var nestedProjectionValidator = new NestedProjectionValidator();
            nestedProjectionValidator.Validate(projector.Body);
        }
    }
}
