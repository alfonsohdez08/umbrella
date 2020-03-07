﻿using System;
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

            throw new InvalidOperationException("The implicit projection should reflect an object, not a primitive.");
        }

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