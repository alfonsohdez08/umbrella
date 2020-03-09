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
        private Stack<Expression> _expressions = new Stack<Expression>();

        public override Expression Rewrite(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var projectionVisitor = new ProjectionVisitor();
            projectionVisitor.Visit(lambda);

            if (projectionVisitor.IsProjectingAnComplexType)
                return expression;

            Visit(lambda.Body);

            Expression projection = null;
            if (_expressions.Count == 1)
            {
                Expression singleExpression = _expressions.Pop();
                string propertyName = string.Empty;
                Type propertyType = lambda.Body.Type;

                if (singleExpression.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression memberExp = (MemberExpression)singleExpression;
                    
                    propertyName = memberExp.Member.Name;
                    //propertyType = lambda.Body.Type;
                }
                else
                {
                    var columnSettings = (ColumnSettings)((ConstantExpression)singleExpression).Value;

                    propertyName = columnSettings.ColumnName;
                    if (string.IsNullOrEmpty(propertyName))
                        throw new InvalidOperationException($"The {typeof(ColumnSettings).Name} does not have a name. Ensure you specify it.");

                    //propertyType = typeof(ColumnSettings);
                }

                var properties = new Dictionary<string, Type>();
                properties.Add(propertyName, propertyType);

                Type anonymousType = AnonymousTypeUtils.CreateType(properties);
                Type[] types = anonymousType.GetProperties().Select(p => p.PropertyType).ToArray();

                projection = Expression.New(anonymousType.GetConstructor(types), new Expression[] {lambda.Body}, new MemberInfo[] {anonymousType.GetProperties()[0]});
            } else if (_expressions.Count > 1)
            {
                throw new InvalidProjectionException("There are multiple members across the projection. Ensure you project a single member, otherwise wrap it using a new operator.", lambda.Body);
            }

            return Expression.Lambda(projection, lambda.Parameters);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Type == typeof(ColumnSettings))
                _expressions.Push(c);

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            // A member accessing chain is allowed: (Customer c) => c.Address.Name

            // Only visits the top node of a member accessing
            _expressions.Push(m);

            return m;
        }

        private class ProjectionVisitor: ExpressionVisitor
        {
            private NewExpression _newExp = null;
            
            /// <summary>
            /// Denotes whether the projection is an explicit object instantiation or not.
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