using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Extensions;

namespace Umbrella.Rewritters
{
    /// <summary>
    /// Rewrittes a projector that has as body a ParameterExpression.
    /// </summary>
    public class ProjectorParameterRewritter: ExpressionVisitor
    {
        private readonly Expression _projectorBody;

        private ProjectorParameterRewritter(Expression projectorBody)
        {
            if (projectorBody == null)
                throw new ArgumentNullException(nameof(projectorBody));

            var parameterExp = projectorBody as ParameterExpression;
            if (parameterExp != null)
                _projectorBody = parameterExp;
            else
                _projectorBody = null;
        }

        public Expression Rewrite() => Visit(_projectorBody);

        /// <summary>
        /// Rewrites an expression of form (object o) => o into an instantiation expression (using new operator).
        /// </summary>
        /// <param name="projectorBody">Projector's body expression.</param>
        /// <returns>If it was not rewritten then returns the same projector; otherwise a new projector.</returns>
        public static Expression Rewrite(Expression projectorBody)
        {
            var projectorParamRewritter = new ProjectorParameterRewritter(projectorBody);
            Expression projectorBodyUpdated = projectorParamRewritter.Rewrite();

            if (projectorBodyUpdated == null)
                return projectorBody;

            return projectorBodyUpdated;
        }

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

                PropertyInfo[] properties = type.GetProperties();
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

            throw new InvalidOperationException("The projector's input type is not a complex type (an object).");
        }

    }
}