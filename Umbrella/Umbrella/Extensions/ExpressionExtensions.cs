using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Extensions
{
    public static class ExpressionExtensions
    {
        public static bool IsObjInstantiationExpression(this Expression e) => e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.MemberInit;

        public static Expression GetLambdaExpressionBody(this Expression e)
        {
            var le = e as LambdaExpression;
            if (le == null)
                throw new InvalidCastException("The underlying expression is not a LambdaExpression.");

            return le.Body;
        }
    }
}
