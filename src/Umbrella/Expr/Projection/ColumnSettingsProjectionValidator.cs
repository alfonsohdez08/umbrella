using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    internal class ColumnSettingsProjectionValidator : ExpressionVisitor, IExpressionValidator
    {
        public void Validate(Expression expression)
        {
            Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var columnSettings = (ColumnSettings)c.Value;
            if (string.IsNullOrEmpty(columnSettings.ColumnName))
                throw new InvalidOperationException($"The {typeof(ColumnSettings).Name} does not have a name. Ensure you specify it.");

            return c;
        }
    }
}
