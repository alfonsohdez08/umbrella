using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Rewritters
{
    internal class MemberAccessProjectionRewritter : ExpressionRewritter
    {
        private Stack<MemberExpression> _members = new Stack<MemberExpression>();

        public override Expression Rewrite(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var projectionVisitor = new ProjectionVisitor();
            projectionVisitor.Visit(lambda);

            if (projectionVisitor.IsProjectingAnComplexType)
                return expression;

            Visit(lambda.Body);

            Expression projection = null;
            if (_members.Count == 1)
            {
                MemberExpression memberExp = _members.Pop();

                var properties = new Dictionary<string, Type>();
                properties.Add(memberExp.Member.Name, lambda.Body.Type);

                Type anonymousType = AnonymousTypeUtils.CreateType(properties);
                Type[] types = anonymousType.GetProperties().Select(p => p.PropertyType).ToArray();

                projection = Expression.New(anonymousType.GetConstructor(types), new Expression[] {lambda.Body}, new MemberInfo[] {anonymousType.GetProperties()[0]});
            } else if (_members.Count > 1)
            {
                throw new InvalidProjectionException("There are multiple members across the projection. Ensure you project a single member, otherwise wrap it using a new operator.", lambda.Body);
            }

            return Expression.Lambda(projection, lambda.Parameters);
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            // A member accessing chain is allowed: (Customer c) => c.Address.Name

            // Only visits the top node of a member accessing
            _members.Push(m);

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