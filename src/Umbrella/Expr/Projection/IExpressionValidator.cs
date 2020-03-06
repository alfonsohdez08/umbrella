using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    internal interface IExpressionValidator
    {
        void Validate(Expression expression);
    }
}