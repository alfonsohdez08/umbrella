using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Rewritters
{
    internal class SingleProjectionRewritter : ExpressionRewritter
    {
        private List<Expression> _expressions;

        public override Expression Rewrite(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var projectionVisitor = new ProjectionVisitor();
            projectionVisitor.Visit(lambda);

            if (projectionVisitor.IsProjectingAnComplexType)
                return expression;

            Expression projection = null;
            _expressions = new List<Expression>();

            try
            {

                Visit(lambda.Body);

                if (_expressions.Count == 1)
                {
                    Expression singleExpression = _expressions[0];
                    string propertyName = string.Empty;
                    Type propertyType = lambda.Body.Type;

                    if (singleExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression memberExp = (MemberExpression)singleExpression;

                        propertyName = memberExp.Member.Name;
                    }
                    else
                    {
                        var columnSettings = (ColumnSettings)((ConstantExpression)singleExpression).Value;

                        propertyName = columnSettings.ColumnName;
                        if (string.IsNullOrEmpty(propertyName))
                            throw new InvalidOperationException($"The {typeof(ColumnSettings).Name} does not have a name. Ensure you specify it.");
                    }

                    // TODO: im planning to downgrade to netstandard 2, so this library would be compaatiable with net framework
                    // i might rewrite this a columnsettings

                    var properties = new Dictionary<string, Type>();
                    properties.Add(propertyName, propertyType);

                    Type anonymousType = AnonymousType.Create(properties);
                    //Type anonymousType = AnonymousTypeUtils.CreateType(properties);
                    Type[] types = anonymousType.GetProperties().Select(p => p.PropertyType).ToArray();

                    projection = Expression.New(anonymousType.GetConstructor(types), new Expression[] { lambda.Body }, new MemberInfo[] { anonymousType.GetProperties()[0] });
                }
                else if (_expressions.Count > 1)
                {
                    throw new InvalidProjectionException("There are multiple members across the projection. Ensure you project a single member, otherwise wrap it using a new operator.", lambda.Body);
                }
            }
            finally
            {
                _expressions = null;
            }

            return Expression.Lambda(projection, lambda.Parameters);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Type == typeof(ColumnSettings))
                _expressions.Add(c);

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            // A member accessing chain is allowed: (Customer c) => c.Address.Name

            // Only visits the top node of a member accessing
            _expressions.Add(m);

            return m;
        }

        private class ProjectionVisitor: ExpressionVisitor
        {
            private NewExpression _newExp = null;
            
            /// <summary>
            /// Denotes whether the projection is a complex type or not. A complex type is basically a structure that has members.
            /// </summary>
            public bool IsProjectingAnComplexType => _newExp != null;

            protected override Expression VisitNew(NewExpression n)
            {
                _newExp = n;

                return base.VisitNew(n);
            }
        }
    }
}