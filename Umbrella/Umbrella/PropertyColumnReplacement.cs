using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expressions;
using Umbrella.Extensions;

namespace Umbrella
{
    public class PropertyColumnReplacement: ExpressionVisitor
    {
        private List<Expression> _candidates;
        private readonly Expression _projectorBody;

        public PropertyColumnReplacement(Expression projectorBody)
        {
            _projectorBody = projectorBody;
        }

        public Expression Replace()
        {
            _candidates = ColumnsNominator.GetCandidates(_projectorBody);

            return Visit(_projectorBody);
        }

        public static Expression ReplacePropertiesByColumns(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;
            var propertyColumnReplacement = new PropertyColumnReplacement(lambdaExp.Body);

            Expression newProjectorBody = propertyColumnReplacement.Replace();

            return Expression.Lambda(newProjectorBody, lambdaExp.Parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_candidates.Contains(node))
            {
                var columnExpression = new ColumnExpression(node);

                return columnExpression;
            }

            return base.Visit(node);
        }
    }
}
