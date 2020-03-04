using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbrella.UnitTests
{
    public static class ExpressionExtensions
    {

        public static Expression[] GetArguments(this NewExpression n)
        {
            Expression[] args = new Expression[n.Arguments.Count];
            n.Arguments.CopyTo(args, 0);

            return args;
        }

        public static Expression GetExpressionByMember(this NewExpression n, string memberName)
        {
            for (int index = 0; index < n.Members.Count; index++)
            {
                MemberInfo member = n.Members[index];
                if (member.Name == memberName)
                    return n.Arguments[index];
            }

            throw new InvalidOperationException($"Member {memberName} not found within the {typeof(NewExpression).Name}.");
        }

        public static Expression GetExpressionByMember(this MemberInitExpression mi, string memberName)
        {
            for (int index = 0; index < mi.Bindings.Count; index++)
            {
                MemberInfo member = mi.Bindings[index].Member;
                if (member.Name == memberName)
                    return ((MemberAssignment)mi.Bindings[index]).Expression;
            }

            throw new InvalidOperationException($"Member {memberName} not found within the {typeof(MemberInitExpression).Name}.");
        }

        public static Expression[] GetBindingExpressions(this MemberInitExpression mi)
        {
            Expression[] expressions = new Expression[mi.Bindings.Count];

            for (int index = 0; index < expressions.Length; index++)
            {
                expressions[index] = ((MemberAssignment)mi.Bindings[index]).Expression;
            }

            return expressions;
        }
    }
}
