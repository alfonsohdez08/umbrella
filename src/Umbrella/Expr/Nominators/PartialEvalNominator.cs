﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbrella.Expr.Nominators
{
    /// <summary>
    /// Subtrees nominator.
    /// </summary>
    internal class PartialEvalNominator : Nominator
    {
        private ParameterExpression _parameter;
        private HashSet<Expression> _nominees;
        private bool _isEvaluable = true;

        /// <summary>
        /// Nominates subtrees that do not reference an specific parameter.
        /// </summary>
        /// <param name="expression">Expression tree (lambda expression).</param>
        /// <returns>A set of potential nodes that can be evaluated.</returns>
        /// <remarks>It nominates top-bottom nodes, so the set would contain all the nodes of a subtree.</remarks>
        public override HashSet<Expression> Nominate(Expression expression)
        {
            LambdaExpression lambda = (LambdaExpression)expression;
            HashSet<Expression> nominees = null;
            
            _nominees = new HashSet<Expression>();
            _parameter = lambda.Parameters[0];

            try
            {
                Visit(expression);

                nominees = _nominees;
            }
            finally
            {
                _nominees = null;
                _parameter = null;
            }

            return nominees;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            bool saveIsEvaluable = _isEvaluable;
            _isEvaluable = true;

            base.Visit(node);

            if (_isEvaluable)
            {
                if (node is ParameterExpression parameter && parameter == _parameter)
                    _isEvaluable = false;
                else
                    _nominees.Add(node);
            }

            _isEvaluable &= saveIsEvaluable;

            return node;
        }
    }
}
