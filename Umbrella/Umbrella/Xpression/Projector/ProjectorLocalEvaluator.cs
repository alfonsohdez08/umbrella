using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;

namespace Umbrella.Xpression.Projector
{
    public class ProjectorLocalEvaluator: ExpressionVisitor
    {
        private HashSet<Expression> _nominees;

        public Expression Evaluate(Expression body, ParameterExpression parameter)
        {
            var localEvalNominator = new ProjectorLocalEvalNominator(parameter);
            _nominees = localEvalNominator.Nominate(body);

            try
            {
                Expression projectorBodyEvaluated = Visit(body);
                if (projectorBodyEvaluated.NodeType == ExpressionType.Constant)
                    throw new InvalidOperationException("The given projector projects a constant, which means the projector parameter was not referenced in the projection.");

                return projectorBodyEvaluated;
            }
            finally
            {
                _nominees = null;
            }
            
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
