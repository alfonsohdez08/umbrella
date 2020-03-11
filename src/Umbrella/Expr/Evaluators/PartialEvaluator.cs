using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;
using Umbrella.Expr.Nominators;
using System.Collections.ObjectModel;

namespace Umbrella.Expr.Evaluators
{
    internal class PartialEvaluator: Evaluator
    {
        private HashSet<Expression> _nominees;

        public override Expression Evaluate(LambdaExpression expression)
        {
            Expression bodyPartiallyEvaluated = null;

            try
            {
                var localEvalNominator = new PartialEvalNominator();
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
                return Evaluate(node);

            return base.Visit(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression mi)
        {
            if (_nominees.Contains(mi))
            {
                return Evaluate(mi);
            }
            else if (_nominees.Contains(mi.NewExpression))
            {
                // Evaluates partially the MemberInitExpression subtree (it checks for the constructor call arguments and
                // the member bindings)

                ReadOnlyCollection<Expression> newExpArgs = Visit(mi.NewExpression.Arguments, Visit);
                NewExpression newExp = Expression.New(mi.NewExpression.Constructor, newExpArgs);

                ReadOnlyCollection<MemberBinding> bindings = Visit(mi.Bindings, VisitMemberBinding);

                return Expression.MemberInit(newExp, bindings);
            }

            return mi;
        }

        private static Expression Evaluate(Expression expression)
        {
            LambdaExpression le = Expression.Lambda(expression, null);
            Delegate del = le.Compile();

            return Expression.Constant(del.DynamicInvoke(), expression.Type);
        }

    }

}
