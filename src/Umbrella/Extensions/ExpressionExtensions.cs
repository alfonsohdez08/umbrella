using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Extensions
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Updates Lambda Expression's body.
        /// </summary>
        /// <param name="lambda">The lambda expression whose body will be replaced.</param>
        /// <param name="body">New Lambda Expression's body.</param>
        public static void UpdateBody(this LambdaExpression lambda, Expression body) => lambda = Expression.Lambda(body, lambda.Parameters);

        /// <summary>
        /// Checks if the projection is flat or not.
        /// </summary>
        /// <param name="lambda">Projector.</param>
        /// <returns>True if it's flat; otherwise false.</returns>
        public static bool IsFlatProjection(this LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if projection holds any reference to the projector's parameter.
        /// </summary>
        /// <param name="lambda">Projector.</param>
        /// <returns>True if do not hold any reference to the projector's parameter; otherwise false.</returns>
        public static bool IsParameterlessProjection(this LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

    }
}
