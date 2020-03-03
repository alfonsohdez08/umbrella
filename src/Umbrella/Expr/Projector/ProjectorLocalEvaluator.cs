using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;
using Umbrella.Expr.Nominators;

namespace Umbrella.Expr.Projector
{
    public class ProjectorLocalEvaluator: ExpressionVisitor
    {
        private HashSet<Expression> _nominees;

        public Expression Evaluate(Expression body, ParameterExpression parameter)
        {
            var localEvalNominator = new ProjectorLocalEvalNominator(parameter);
            _nominees = localEvalNominator.Nominate(body);

            Expression newProjectorBody = null;
            try
            {
                newProjectorBody = Visit(body);
            }
            finally
            {
                _nominees = null;
            }

            return newProjectorBody;
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
