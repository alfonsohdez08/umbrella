using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;
using Umbrella.Expr.Nominators;
using System.Collections.ObjectModel;

namespace Umbrella.Expr.Evaluators
{
    /// <summary>
    /// Subtree evaluator.
    /// </summary>
    internal class PartialEvaluator: Evaluator
    {
        /// <summary>
        /// Potential nodes for evaluation.
        /// </summary>
        private HashSet<Expression> _nominees;

        /// <summary>
        /// Traverses over the expression tree in order to find subtrees that can be evaluated.
        /// </summary>
        /// <param name="expression">Expression tree.</param>
        /// <returns>A modified tree if evaluated any subtree; otherwise the same tree.</returns>
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

            // The node that goes through this condition is the topmost node of a subtree
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
                // Evaluates partially the MemberInitExpression subtree (it checks for the constructor call arguments and the member bindings)

                ReadOnlyCollection<Expression> newExpArgs = Visit(mi.NewExpression.Arguments, Visit);
                NewExpression newExp = null;

                if (mi.NewExpression.Constructor != null)
                    newExp = Expression.New(mi.NewExpression.Constructor, newExpArgs);
                else
                    newExp = Expression.New(mi.NewExpression.Type);

                ReadOnlyCollection<MemberBinding> bindings = Visit(mi.Bindings, VisitMemberBinding);

                return Expression.MemberInit(newExp, bindings);
            }

            return mi;
        }

        /// <summary>
        /// Evaluates the given expression.
        /// </summary>
        /// <param name="expression">Expression that would be evaluated.</param>
        /// <returns>A expression that denotes a constant.</returns>
        private static Expression Evaluate(Expression expression)
        {
            LambdaExpression le = Expression.Lambda(expression, null);
            Delegate del = le.Compile();

            return Expression.Constant(del.DynamicInvoke(), expression.Type);
        }

    }

}
