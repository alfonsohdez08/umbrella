using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Rewritters
{
    internal class MemberAccessProjectedRewritter : ExpressionRewritter
    {
        private Stack<MemberExpression> _accesses = new Stack<MemberExpression>();

        public override Expression Rewrite(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var projectionVisitor = new ProjectionVisitor();
            projectionVisitor.Visit(lambda);

            if (projectionVisitor.IsProjectingAnComplexType)
                return expression;

            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                Visit(lambda.Body);

            Expression projection = null;
            if (_accesses.Count == 1)
            {
                MemberExpression memberExp = _accesses.Pop();

                var properties = new Dictionary<string, Type>();
                properties.Add(memberExp.Member.Name, memberExp.Type);

                Type anonymousType = AnonymousType.CreateAnonymousType(properties);
                Type[] types = anonymousType.GetMembers().Select(m => m.GetType()).ToArray();

                projection = Expression.New(anonymousType.GetConstructor(types), new Expression[] {lambda.Body}, new MemberInfo[] { memberExp.Member });
            } else if (_accesses.Count > 1)
            {
                //throw exception
            }

            return projection;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            // A member accessing chain is allowed: (Customer c) => c.Address.Name

            // Only visits the top node of a member accessing
            _accesses.Push(m);

            return m;
        }

        private class ProjectionVisitor: ExpressionVisitor
        {
            private NewExpression _newExp = null;
            public bool IsProjectingAnComplexType => _newExp != null;

            protected override Expression VisitNew(NewExpression n)
            {
                _newExp = n;

                return base.VisitNew(n);
            }
        }
    }
}