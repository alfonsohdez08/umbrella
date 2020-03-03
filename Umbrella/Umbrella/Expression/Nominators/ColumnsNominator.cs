﻿using System.Collections.Generic;
using System.Linq.Expressions;
using Umbrella.Extensions;

namespace Umbrella
{
    public class ColumnsNominator : Nominator
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
            
            if (node.NodeType == ExpressionType.New)
                _isPartOfColumn = false;
            else
                _nominees.Add(node);

            _isPartOfColumn &= saveIsPartOfColumn;

            return node;
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
    }
}

