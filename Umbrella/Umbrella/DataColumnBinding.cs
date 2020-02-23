﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbrella
{
    internal static class DataColumnBinding
    {
        private class ProjectionVisitor : ExpressionVisitor
        {
            private readonly Dictionary<DataColumn, Delegate> _bindings;

            private readonly ParameterExpression _parameterExp;
            private readonly Expression _expression;

            public ProjectionVisitor(Expression expression)
            {
                _bindings = new Dictionary<DataColumn, Delegate>();

                var lambdaExp = (LambdaExpression)expression;

                _parameterExp = lambdaExp.Parameters[0];
                _expression = lambdaExp.Body;
            }

            public Dictionary<DataColumn, Delegate> GetBindings()
            {
                Visit(_expression);

                return _bindings;
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                MemberInfo[] members = p.Type.GetProperties();
                Expression[] expressions = new Expression[members.Length];

                for (int index = 0; index < members.Length; index++)
                    expressions[index] = Expression.MakeMemberAccess(p, members[index]);

                BindColumnsExpressions(members, expressions);

                return p;
            }

            protected override Expression VisitMemberInit(MemberInitExpression m)
            {
                Expression[] expressions = new Expression[m.Bindings.Count];
                MemberInfo[] members = new MemberInfo[m.Bindings.Count];

                int count = 0;
                foreach (MemberBinding mb in m.Bindings)
                {
                    MemberAssignment ma = mb as MemberAssignment;
                    if (ma == null)
                    {
                        var invalidOpException = new InvalidOperationException("The member initialization only supports member assignment.");
                        invalidOpException.HelpLink = "https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.memberassignment?view=netstandard-2.0";

                        throw invalidOpException;
                    }

                    members[count] = ma.Member;
                    expressions[count] = ma.Expression;

                    count++;
                }

                BindColumnsExpressions(members, expressions);

                return m;
            }

            protected override Expression VisitNew(NewExpression n)
            {
                Expression[] expressions = new Expression[n.Arguments.Count];
                n.Arguments.CopyTo(expressions, 0);

                MemberInfo[] members = new MemberInfo[n.Members.Count];
                n.Members.CopyTo(members, 0);

                BindColumnsExpressions(members, expressions);

                return n;
            }

            private void BindColumnsExpressions(MemberInfo[] members, Expression[] expressions)
            {
                CheckIfHasNestedObjInstantiator(expressions);

                for (int index = 0; index < members.Length; index++)
                {
                    PropertyInfo property = (PropertyInfo)members[index];
                    Expression expression = expressions[index];

                    LambdaExpression lambdaExp = Expression.Lambda(expression, _parameterExp);
                    var column = new DataColumn(property.Name, property.PropertyType);

                    _bindings.Add(column, lambdaExp.Compile());
                }
            }

            /// <summary>
            /// Checks for nested object instatiator (expressions that denotes object instatiation).
            /// </summary>
            /// <param name="expressions">Set of expressions (immediate node expressions).</param>
            private void CheckIfHasNestedObjInstantiator(Expression[] expressions)
            {
                for (int index = 0; index < expressions.Length; index++)
                    if (expressions[index].IsObjectInstantiator())
                        throw new InvalidOperationException("The projector can not have nested object instantiator.");
            }
        }

        public static Dictionary<DataColumn, Delegate> GetColumnsBinded(Expression projector)
        {
            var projectionVisitor = new ProjectionVisitor(projector);

            return projectionVisitor.GetBindings();
        }
    }

    internal static class ExpressionExtensions
    {
        public static bool IsObjectInstantiator(this Expression e)
        {
            return e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.MemberInit;
        }
    }
}