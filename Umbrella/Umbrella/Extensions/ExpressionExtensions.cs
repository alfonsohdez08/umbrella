using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Extensions
{
    public static class ExpressionExtensions
    {
        public static bool IsObjInstantiationExpression(this Expression e) => e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.MemberInit;

        public static bool IsValidProjector(this Expression e) => e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.MemberInit; 
    }
}