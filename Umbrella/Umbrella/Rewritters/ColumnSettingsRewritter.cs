using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Extensions;

namespace Umbrella.Rewritters
{
    public class ColumnSettingsRewritter: ExpressionVisitor
    {
        private readonly Expression _projectorBody;

        public ColumnSettingsRewritter(Expression projectorBody)
        {
            _projectorBody = projectorBody;
        }

        public static Expression Rewrite(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            var columnSettingsRewritter = new ColumnSettingsRewritter(lambdaExp.Body);
            Expression newProjectorBody = columnSettingsRewritter.Rewrite();

            return Expression.Lambda(newProjectorBody, lambdaExp.Parameters);
        }

        public Expression Rewrite()
        {
            return Visit(_projectorBody);
        }

        protected override Expression VisitMethodCall(MethodCallExpression mc)
        {
            if (mc.Type == typeof(ColumnSettings) && mc.Method.Name == "Build")
            {
                var expQuoted = (UnaryExpression)mc.Arguments[0];
                var mapperLambdaExp = (LambdaExpression)expQuoted.Operand;

                // why this throws an exception? (research this)
                //var x = Expression.Call(null, mc.Method, new Expression[] {mapperLambdaExp});

                var columnSettings = typeof(ColumnSettings).GetMethod("Build").MakeGenericMethod(mapperLambdaExp.Body.Type).Invoke(null, new object[] { mapperLambdaExp });

                return Expression.Constant(columnSettings, typeof(ColumnSettings));
            }else if (mc.Type == typeof(ColumnSettings))
            {
                Expression e = base.VisitMethodCall(mc);
                LambdaExpression le = Expression.Lambda(e);

                var columnSettings = (ColumnSettings)le.Compile().DynamicInvoke();

                return Expression.Constant(columnSettings, typeof(ColumnSettings));
            }

            return base.VisitMethodCall(mc);
        }

    }
}