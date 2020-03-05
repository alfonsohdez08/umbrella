using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Extensions;

namespace Umbrella.Expr.Rewritters
{
    /// <summary>
    /// Rewrittes a projector that has as body a ParameterExpression.
    /// </summary>
    internal class ParameterProjectedRewritter : ExpressionRewritter
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
            else if (type.IsClass)
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                NewExpression ne = Expression.New(constructor);

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

            throw new InvalidOperationException("The projected parameter is not a complex type. Please use the new operator for project the non complex parameter.");
        }

        public override Expression Rewrite(Expression projection)
        {
            if (projection is ParameterExpression)
                return Visit(projection);

            return projection;
        }
    }
}