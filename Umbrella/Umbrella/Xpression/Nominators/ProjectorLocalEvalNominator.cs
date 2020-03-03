using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbrella.Xpression.Nominators
{
    public class ProjectorLocalEvalNominator : Nominator
    {
        private readonly ParameterExpression _parameter;

        private HashSet<Expression> _nominees = new HashSet<Expression>();

        private bool _isEvaluableLocally = true;

        public ProjectorLocalEvalNominator(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        public override HashSet<Expression> Nominate(Expression expression)
        {
            try
            {
                Visit(expression);

                return _nominees;
            }
            finally
            {
                _nominees = new HashSet<Expression>();
            }
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            bool saveIsEvaluableLocally = _isEvaluableLocally;
            _isEvaluableLocally = true;

            base.Visit(node);

            if (node is ParameterExpression parameter && parameter == _parameter)
                _isEvaluableLocally = false;

            if (_isEvaluableLocally)
                _nominees.Add(node);

            _isEvaluableLocally &= saveIsEvaluableLocally;

            return node;
        }
    }
}
