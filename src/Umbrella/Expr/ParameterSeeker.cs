using System.Linq.Expressions;

namespace Umbrella.Expr
{
    public class ParameterSeeker: ExpressionVisitor
    {
        private ParameterExpression _parameter;
        private bool _foundParameter = false;

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