using System.Linq.Expressions;

namespace Umbrella.Xpression
{
    public class ParameterReferencesFinder: ExpressionVisitor
    {
        private ParameterExpression _parameter;
        private bool _foundParameter = false;

        public bool Find(Expression expression, ParameterExpression parameter)
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
            if (c.Type == typeof(ColumnSettings))
            {
                var columnSettings = (ColumnSettings)c.Value;
                Visit(columnSettings.Mapper);
            }

            return c;
        }
    }
}