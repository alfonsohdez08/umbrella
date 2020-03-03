using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Xpression.Rewritters
{
    public abstract class ExpressionRewritter: ExpressionVisitor
    {
        public abstract Expression Rewrite(Expression expression);
    }
}
