using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Umbrella.Expr.Column
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

        public ColumnExpression(System.Linq.Expressions.Expression columnDefinition)
        {
            _type = columnDefinition.Type;

            ColumnDefinition = columnDefinition;
        }

    }
}
