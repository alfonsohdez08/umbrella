using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella
{
    public class LocalEvaluator: ExpressionVisitor
    {
        private readonly List<Expression> _candidates;
        private readonly Expression _projectorBody;

        public LocalEvaluator(List<Expression> candidates, Expression projectorBody)
        {
            _candidates = candidates;
            _projectorBody = projectorBody;
        }

        public static Expression Evaluate(Expression projectorBody, ParameterExpression projectorParameter)
        {
            var localEvalNominator = new LocalEvaluationNominator(projectorBody, projectorParameter);
            localEvalNominator.Nominate();

            List<Expression> candidates = localEvalNominator.Candidates;
            var localEval = new LocalEvaluator(candidates, projectorBody);
            
            Expression projectorBodyUpdated = localEval.Evaluate();

            if (projectorBodyUpdated.NodeType == ExpressionType.Constant)
                throw new InvalidOperationException("The projector does not have any reference to the input type.");

            return projectorBodyUpdated;
        }

        public Expression Evaluate()
        {
            return Visit(_projectorBody);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_candidates.Contains(node))
            {
                LambdaExpression le = Expression.Lambda(node, null);
                Delegate del = le.Compile();

                return Expression.Constant(del.DynamicInvoke());
            }

            return base.Visit(node);
        }

        private class LocalEvaluationNominator: ExpressionVisitor
        {
            private readonly ParameterExpression _projectorParameter;
            private readonly Expression _projectorBody;

            private bool _isLocalEvaluable = true;
            public List<Expression> Candidates { get; private set; } = new List<Expression>();

            public LocalEvaluationNominator(Expression projectorBody, ParameterExpression projectorParameter)
            {
                _projectorParameter = projectorParameter;
                _projectorBody = projectorBody;
            }

            public void Nominate()
            {
                Visit(_projectorBody);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return node;

                bool saveIsLocalEvaluable = _isLocalEvaluable;
                _isLocalEvaluable = true;

                base.Visit(node);

                if (node as ParameterExpression != null && ((ParameterExpression)node) == _projectorParameter)
                    _isLocalEvaluable = false;

                if (_isLocalEvaluable)
                    Candidates.Add(node);

                _isLocalEvaluable &= saveIsLocalEvaluable;

                return node;
            }
        }

    }
}
