using System.Linq.Expressions;

namespace Umbrella.Expr
{
    /// <summary>
    /// Seeker for expression parameters.
    /// </summary>
    internal class ParameterSeeker: ExpressionVisitor
    {
        private ParameterExpression _parameter;
        private bool _foundParameter = false;

        /// <summary>
        /// Checks wheter a specific instance of a parameter exist or not within an expression.
        /// </summary>
        /// <param name="expression">Expression for inspection.</param>
        /// <param name="parameter">Parameter instance that would be seeked.</param>
        /// <returns>True if found; otherwise false.</returns>
        public bool Exists(Expression expression, ParameterExpression parameter)
        {
            bool foundParameter = false;

            try
            {
                _parameter = parameter;
                Visit(expression);

                foundParameter = _foundParameter;
            }
            finally
            {
                _foundParameter = false;
            }

            return foundParameter;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_parameter == p)
                _foundParameter = true;

            return p;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is ColumnSettings columnSettings)
                Visit(columnSettings.Mapper);

            return c;
        }
    }
}