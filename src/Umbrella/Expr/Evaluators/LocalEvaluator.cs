using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;
using Umbrella.Expr.Nominators;

namespace Umbrella.Expr.Evaluators
{
    internal class LocalEvaluator: Evaluator
    {
        private HashSet<Expression> _nominees;

        public override Expression Evaluate(LambdaExpression expression)
        {
            Expression bodyPartiallyEvaluated = null;

            try
            {
                var localEvalNominator = new LocalEvalNominator();
                _nominees = localEvalNominator.Nominate(expression);

                bodyPartiallyEvaluated = Visit(expression.Body);
            }
            finally
            {
                _nominees = null;
            }

            return Expression.Lambda(bodyPartiallyEvaluated, expression.Parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_nominees.Contains(node))
            {
                LambdaExpression le = Expression.Lambda(node, null);
                Delegate del = le.Compile();

                return Expression.Constant(del.DynamicInvoke());
            }

            return base.Visit(node);
        }
    }

}
