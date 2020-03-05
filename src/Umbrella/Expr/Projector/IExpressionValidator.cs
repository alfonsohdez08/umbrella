using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projector
{
    internal interface IExpressionValidator
    {
        void Validate(Expression expression);
    }
}