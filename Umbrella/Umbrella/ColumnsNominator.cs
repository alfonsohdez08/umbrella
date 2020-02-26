using System.Collections.Generic;
using System.Linq.Expressions;
using Umbrella.Extensions;

namespace Umbrella
{
    public class ColumnsNominator : ExpressionVisitor
    {
        private readonly Expression _projector;

        private readonly List<Expression> _expressions;
        private bool _isPartOfColumn = true;

        public ColumnsNominator(Expression projectorBody)
        {
            _projector = projectorBody;
            _expressions = new List<Expression>();
        }

        public void Nominate()
        {
            Visit(_projector);
        }

        public static List<Expression> GetCandidates(Expression projectorBody)
        {
            var columnsNominator = new ColumnsNominator(projectorBody);
            columnsNominator.Nominate();

            return columnsNominator._expressions;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            bool saveIsPartOfColumn = _isPartOfColumn;
            _isPartOfColumn = true;

            base.Visit(node);

            if (node.IsObjInstantiationExpression())
                _isPartOfColumn = false;
            else
                _expressions.Add(node);

            _isPartOfColumn &= saveIsPartOfColumn;

            return node;
        }
    }
}

