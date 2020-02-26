using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expressions
{
    public enum DataTableExpression
    {
        Column = 1000
    }

    public class ColumnExpression : Expression
    {
        private readonly Type _type;
        public Expression ColumnDefinition { get; private set; }
        public override Type Type => _type;
        public override ExpressionType NodeType => (ExpressionType)DataTableExpression.Column;

        public ColumnExpression(Expression columnDefinition)
        {
            _type = columnDefinition.Type;

            ColumnDefinition = columnDefinition;
        }

    }
}
