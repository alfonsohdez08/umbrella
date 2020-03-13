using System.Collections.Generic;
using System.Linq.Expressions;
using Umbrella.Extensions;

namespace Umbrella.Expr.Nominators
{
    /// <summary>
    /// DataTable's columns nominator.
    /// </summary>
    internal class ColumnsNominator : Nominator
    {
        private HashSet<Expression> _nominees = new HashSet<Expression>();
        private bool _isPartOfColumn = true;

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            bool saveIsPartOfColumn = _isPartOfColumn;
            _isPartOfColumn = true;

            base.Visit(node);

            if (_isPartOfColumn)
            {
                if (node.NodeType == ExpressionType.New)
                    _isPartOfColumn = false;
                else
                    _nominees.Add(node);
            }

            _isPartOfColumn &= saveIsPartOfColumn;

            return node;
        }

        /// <summary>
        /// Nominates all the subtrees that do not denote a type instantiation (meaning, no NewExpressions) as columns for the DataTable.
        /// </summary>
        /// <param name="expression">Projection.</param>
        /// <returns>A set of nodes that potentially represent the DataTable's columns.</returns>
        public override HashSet<Expression> Nominate(Expression expression)
        {
            _nominees = new HashSet<Expression>();

            HashSet<Expression> nominees = null;

            try
            {
                Visit(expression);

                nominees = _nominees;
            }
            finally
            {
                _nominees = null;
            }

            return nominees;
        }
    }
}

