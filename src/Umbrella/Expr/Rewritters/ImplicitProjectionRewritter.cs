using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Umbrella.Extensions;

namespace Umbrella.Expr.Rewritters
{
    /// <summary>
    /// Rewritter for implicit projections.
    /// </summary>
    internal class ImplicitProjectionRewritter : ExpressionRewritter
    {
        protected override Expression VisitParameter(ParameterExpression p)
        {
            Type type = p.Type;
            if (type.IsAnonymousType())
            {
                PropertyInfo[] properties = type.GetProperties();
                Expression[] arguments = new Expression[properties.Length];
                Type[] propertyTypes = new Type[properties.Length];

                for (int index = 0; index < propertyTypes.Length; index++)
                {
                    propertyTypes[index] = properties[index].PropertyType;
                    arguments[index] = Expression.MakeMemberAccess(p, properties[index]);
                }
                ConstructorInfo constructor = type.GetConstructor(propertyTypes);

                NewExpression ne = Expression.New(constructor, arguments, properties);

                return ne;
            }
            else if (type.IsClass || type.IsStruct())
            {
                NewExpression ne = Expression.New(type);

                PropertyInfo[] properties = type.GetProperties().Where(pr => pr.CanWrite && pr.PropertyType.IsBuiltInType()).ToArray();
                List<MemberBinding> memberBindings = new List<MemberBinding>();

                for (int index = 0; index < properties.Length; index++)
                {
                    MemberExpression me = Expression.MakeMemberAccess(p, properties[index]);
                    MemberBinding mb = Expression.Bind(properties[index], me);

                    memberBindings.Add(mb);
                }

                MemberInitExpression mi = Expression.MemberInit(ne, memberBindings);

                return mi;
            }

            throw new InvalidOperationException("The implicit projection should reflect a composite type, not a primitive/simple.");
        }

        /// <summary>
        /// Rewrittes an implicit projection (a projector that projects simply its parameter) into an explicit one (one that uses the new operator).
        /// </summary>
        /// <param name="expression">Whole projector.</param>
        /// <returns>A new projector if it was an implicit projection; otherwise the same projector.</returns>
        public override Expression Rewrite(Expression expression)
        {
            var projector = (LambdaExpression)expression;
            if (projector.Body.NodeType == ExpressionType.Parameter)
            {
                Expression explicitProjection = Visit(projector.Body);

                return Expression.Lambda(explicitProjection, projector.Parameters);
            }

            return expression;
        }
    }
}