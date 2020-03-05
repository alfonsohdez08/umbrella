using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Evaluators
{
    internal abstract class Evaluator: ExpressionVisitor
    {
        public abstract Expression Evaluate(LambdaExpression expression);
    }
}
