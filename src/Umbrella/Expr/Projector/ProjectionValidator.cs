using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projector
{
    internal class ProjectionValidator : IExpressionValidator
    {
        /// <summary>
        /// Validates that the projection is valid. A projection is valid if it's a flat projection and references at least one time the projector's parameter.
        /// </summary>
        /// <param name="expression">The projector.</param>
        public void Validate(Expression expression)
        {
            var paramProjectionValidator = new ParameterProjectionValidator();
            paramProjectionValidator.Validate(expression);

            var flatProjectionValidator = new FlatProjectionValidator();
            flatProjectionValidator.Validate(expression);

            //// should i check this using a visitor?
            //// (Product p) => (object)(p => new {p.Id, p.UnitPrice}) the projector is trapped in a cast expression (unaryexpression)
            //if (!(projector.Body.NodeType == ExpressionType.New || projector.Body.NodeType == ExpressionType.MemberInit))
            //    throw new ArgumentException("The given projector should denote an object instantiation.");
        }
    }
}
