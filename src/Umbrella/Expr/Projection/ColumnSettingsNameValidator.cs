using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    /// <summary>
    /// Validator for ColumnSettings name.
    /// </summary>
    internal class ColumnSettingsNameValidator : ExpressionVisitor, IExpressionValidator
    {
        /// <summary>
        /// Checks if the ColumnSettings instance has already a name for the column.
        /// </summary>
        /// <param name="expression">Projector.</param>
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
