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
            var localEvalNominator = new LocalEvalNominator();
            _nominees = localEvalNominator.Nominate(expression);

            Expression body = null;
            try
            {
                body = Visit(expression.Body);
            }
            finally
            {
                _nominees = null;
            }

            return Expression.Lambda(body, expression.Parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_nominees.Contains(node) && node.Type.IsBuiltInType())
            {
                LambdaExpression le = Expression.Lambda(node, null);
                Delegate del = le.Compile();

                return Expression.Constant(del.DynamicInvoke());
            }

            return base.Visit(node);
        }
    }

}
