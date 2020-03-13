using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Expr.Projection
{
    internal class ProjectionValidator : IExpressionValidator
    {
        /// <summary>
        /// Checks whether the projection is valid or not. A projection is valid if it's a flat projection and references at least one time the projector's parameter.
        /// </summary>
        /// <param name="expression">Projector.</param>
        public void Validate(Expression expression)
        {
            // Checks if the lambda's expression parameter is referenced within the projection
            var paramProjectionValidator = new ParameterReferencesValidator();
            paramProjectionValidator.Validate(expression);

            // Determines if the projection denotes a composite type or not
            var compositeTypeVisitor = new CompositeTypeVisitor();
            compositeTypeVisitor.Visit(expression);

            if (!compositeTypeVisitor.IsProjectingACompositeType)
            {
                // It means it's a single member projection, so it checks if the ColumnSettings has a name set
                var columnSettingsProjValidator = new ColumnSettingsNameValidator();
                columnSettingsProjValidator.Validate(expression);
            }
            else
            {
                var flatProjectionValidator = new FlatProjectionValidator();
                flatProjectionValidator.Validate(expression);
            }
        }
    }
}
