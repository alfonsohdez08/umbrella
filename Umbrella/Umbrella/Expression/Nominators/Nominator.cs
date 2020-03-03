using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbrella
{
    public abstract class Nominator: ExpressionVisitor
    {
        public abstract HashSet<Expression> Nominate(Expression expression);
    }
}
