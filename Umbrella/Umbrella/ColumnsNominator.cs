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

        public ColumnsNominator(Expression projector)
        {
            _projector = projector.GetLambdaExpressionBody();
            _expressions = new List<Expression>();
        }

        public void Nominate()
        {
            Visit(_projector);
        }

        public static List<Expression> GetCandidates(Expression projector)
        {
            var columnsNominator = new ColumnsNominator(projector);
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

