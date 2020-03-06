using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;

namespace Umbrella.Expr.Evaluators
{
    internal class ColumnSettingsEvaluator: Evaluator
    {
        protected override Expression VisitMethodCall(MethodCallExpression mc)
        {
            if (mc.Type == typeof(ColumnSettings) && mc.Method.Name == "Build")
            {
                var expQuoted = (UnaryExpression)mc.Arguments[0];
                var mapperLambdaExp = (LambdaExpression)expQuoted.Operand;

                var columnSettings = typeof(ColumnSettings).GetMethod("Build").MakeGenericMethod(mapperLambdaExp.Body.Type).Invoke(null, new object[] { mapperLambdaExp });

                return Expression.Constant(columnSettings, typeof(ColumnSettings));
            }
            else if (mc.Type == typeof(ColumnSettings))
            {
                Expression e = base.VisitMethodCall(mc);
                LambdaExpression le = Expression.Lambda(e);

                var columnSettings = (ColumnSettings)le.Compile().DynamicInvoke();

                return Expression.Constant(columnSettings, typeof(ColumnSettings));
            }

            return base.VisitMethodCall(mc);
        }

        public override Expression Evaluate(LambdaExpression expression)
        {
            Expression body = Visit(expression.Body);

            return Expression.Lambda(body, expression.Parameters);
        }
    }
}