using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbrella.App
{
    public class ProjectionVisitor: ExpressionVisitor
    {
        public List<Expression> Candidates { get; private set; } = new List<Expression>();

        private bool _isPartOfColumn = true;

        private ProjectionVisitor()
        {

        }

        // I strip the lambda expression for get the parameter expression
        public static ColumnCandidates GetCandidates(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            var projectionVisitor = new ProjectionVisitor();
            projectionVisitor.Visit(lambdaExp.Body);

            List<Expression> candidates = projectionVisitor.Candidates;

            return new ColumnCandidates(lambdaExp.Parameters[0], candidates);
        }

        //I assume I started from the Lambda's body
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            bool saveIsPartOfColumn = _isPartOfColumn;
            _isPartOfColumn = true;

            base.Visit(node);

            if (!(node.NodeType == ExpressionType.New || node.NodeType == ExpressionType.MemberInit))
            {
                Candidates.Add(node);
            }
            else
            {
                _isPartOfColumn = false;
            }

            _isPartOfColumn &= saveIsPartOfColumn;

            return node;
        }

    }

    public class ColumnCandidates
    {
        public ParameterExpression Parameter { get; private set; }
        public List<Expression> Candidates { get; private set; }

        public ColumnCandidates(ParameterExpression parameter, List<Expression> candidates)
        {
            Parameter = parameter;
            Candidates = candidates;
        }
    }
}
