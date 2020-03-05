using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbrella.Expr.Nominators
{
    internal class LocalEvalNominator : Nominator
    {
        private ParameterExpression _parameter;
        private HashSet<Expression> _nominees = new HashSet<Expression>();
        private bool _isEvaluableLocally = true;

        public override HashSet<Expression> Nominate(Expression expression)
        {
            LambdaExpression lambda = (LambdaExpression)expression;
            HashSet<Expression> nominees = null;

            _parameter = lambda.Parameters[0];

            try
            {
                Visit(expression);

                nominees = _nominees;
            }
            finally
            {
                _nominees = new HashSet<Expression>();
                _parameter = null;
            }

            return nominees;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            bool saveIsEvaluableLocally = _isEvaluableLocally;
            _isEvaluableLocally = true;

            base.Visit(node);

            if (_isEvaluableLocally)
            {
                if (node is ParameterExpression parameter && parameter == _parameter)
                    _isEvaluableLocally = false;
                else
                    _nominees.Add(node);
            }

            _isEvaluableLocally &= saveIsEvaluableLocally;

            return node;
        }
    }
}
