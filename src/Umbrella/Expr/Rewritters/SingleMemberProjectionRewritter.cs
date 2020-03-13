using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;

namespace Umbrella.Expr.Rewritters
{
    /// <summary>
    /// Rewrites a projection that produces a single member.
    /// </summary>
    internal class SingleMemberProjectionRewritter : ExpressionRewritter
    {
        private List<Expression> _expressions;

        /// <summary>
        /// Rewrites a projection that produces a single member to a projection that produces a member of type ColumnSettings.
        /// </summary>
        /// <param name="expression">Projector.</param>
        /// <returns>A modified expression if projection produces a single member; otherwise the same expression.</returns>
        public override Expression Rewrite(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var compositeTypeVisitor = new CompositeTypeVisitor();
            compositeTypeVisitor.Visit(lambda);

            if (compositeTypeVisitor.IsProjectingACompositeType)
                return expression;

            Expression projection = null;
            _expressions = new List<Expression>();

            try
            {

                Visit(lambda.Body);

                if (_expressions.Count == 1)
                {

                    MemberExpression memberExp = (MemberExpression)_expressions[0];
                    string propertyName = memberExp.Member.Name;

                    var columnSettings = ((ColumnSettings)typeof(ColumnSettings)
                        .GetMethod("Build")
                        .MakeGenericMethod(new Type[] { lambda.Body.Type })
                        .Invoke(null, new object[] { Expression.Lambda(lambda.Body) }))
                        .Name(propertyName);

                    projection = Expression.Constant(columnSettings, typeof(ColumnSettings));
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

            if (projection == null)
                return expression;

            return Expression.Lambda(projection, lambda.Parameters);
        }

        //protected override Expression VisitConstant(ConstantExpression c)
        //{
        //    if (c.Type == typeof(ColumnSettings))
        //        _expressions.Add(c);

        //    return c;
        //}

        protected override Expression VisitMember(MemberExpression m)
        {
            // A member accessing chain is allowed: (Customer c) => c.Address.Name

            // Only visits the top node of a member accessing
            _expressions.Add(m);

            return m;
        }
    }
}