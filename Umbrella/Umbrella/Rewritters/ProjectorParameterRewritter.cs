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
        private readonly Expression _projector;

        private ProjectorParameterRewritter(Expression projector)
        {
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            var parameterExp = projector as ParameterExpression;
            if (parameterExp != null)
                _projector = parameterExp;
            else
                _projector = null;
        }

        public Expression Rewrite() => Visit(_projector);

        /// <summary>
        /// Rewrites an expression of form (object o) => o into an instatiation expression (using new operator).
        /// </summary>
        /// <param name="projector">Projector.</param>
        /// <returns>If it was not rewritten then returns the same projector; otherwise a new projector.</returns>
        public static Expression Rewrite(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            var projectorParamRewritter = new ProjectorParameterRewritter(lambdaExp.Body);
            Expression newProjector = projectorParamRewritter.Rewrite();

            if (newProjector == null)
                return projector;

            return Expression.Lambda(newProjector, lambdaExp.Parameters);
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