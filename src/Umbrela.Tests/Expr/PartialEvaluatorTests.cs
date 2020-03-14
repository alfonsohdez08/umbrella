using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Evaluators;
using Umbrella.Expr.Projection;
using Umbrella.Tests.Mocks;
using Xunit;

namespace Umbrella.Tests.Expr
{
    public class PartialEvaluatorTests
    {
        private PartialEvaluator _partialEvaluator = new PartialEvaluator();

        [Fact(DisplayName = "When pass an instance method that one of its parameter is a projector's parameter member, it should not evaluate the instance method and let the node intact in the tree.")]
        public void Evaluate_PassAnInstanceMethodInTheProjectorThatReferenceTheProjectoParameter_ShouldNotExecuteTheMethodLocally()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { PersonId = p.Id, Taxes = new TaxService().GetTaxes(p.Id)};

            // Act
            var projectorEvaluated = (LambdaExpression)_partialEvaluator.Evaluate(projector);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated.Body).GetArguments();

            Assert.True(newExpArgs[1].NodeType == ExpressionType.Call);
        }

        [Fact(DisplayName = "When pass a parameterless static method within the projection, it should execute the static method and replace the method's calling by its result as a constant within the projection.")]
        public void Evaluate_PassAStaticMethodInTheProjectorThatDoNotReferenceTheProjectorParameter_ShouldExecuteTheMethodAndTreatItsResultAsAConstant()
        {
            Expression<Func<Person, dynamic>> projector = p => new { PersonId = p.Id, TaxCounselor = TaxService.GetClosestTaxCounselor() };
            
            // Act
            var projectorEvaluated = (LambdaExpression)_partialEvaluator.Evaluate(projector);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated.Body).GetArguments();

            Assert.True(newExpArgs[1] is ConstantExpression constantExp && (string)constantExp.Value == TaxService.GetClosestTaxCounselor());
        }

        [Fact(DisplayName = "When pass an inline condition that do not reference the projector's parameter within the projection, it should execute it locally and replace the inline condition by its value as a constant within the projection.")]
        public void Evaluate_PassAnInlineConditionThatDoNotReferenceTheProjectorParameter_ShouldExecuteItLocallyAndTreatItAsAConstant()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, CanGetIncomingTax = new TaxService().IsIncomingTaxSeason() ? true : false};

            // Act
            var projectorEvaluated = (LambdaExpression)_partialEvaluator.Evaluate(projector);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated.Body).GetArguments();

            bool canGetIncomingTax = new TaxService().IsIncomingTaxSeason() ? true : false;
            Assert.True(newExpArgs[1] is ConstantExpression constantExp && (bool)constantExp.Value == canGetIncomingTax);
        }

        [Fact(DisplayName = "When pass an inline condition that references the projector's parameter within the projection, it should not execute it locally.")]
        public void Evaluate_PassAnInlineConditionThatReferenceTheProjectorParameter_ShouldNotExecuteItLocally()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, CanGetIncomingTax = TaxService.IsTaxAvailable(p.Id) ? true : false };

            // Act
            var projectorEvaluated = (LambdaExpression)_partialEvaluator.Evaluate(projector);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated.Body).GetArguments();

            Assert.True(newExpArgs[1] is ConditionalExpression);
        }

        [Fact(DisplayName = "When pass an inline condition that can be evaluable but the return value can't be evaluated, it should keep the conditional expression in the tree.")]
        public void Evaluate_PassAnInlineConditionThatCanBeEvaluableButTheReturnValueCant_ShouldKeepTheConditionalInTheTree()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector =
                p => new { p.Id,
                    Taxes = TaxService.GetClosestTaxCounselor() == TaxService.GetClosestTaxCounselor() ? new TaxService().GetTaxes(p.Id) : 0 };

            // Act
            var projectorEvaluated = (LambdaExpression)_partialEvaluator.Evaluate(projector);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated.Body).GetArguments();

            Assert.True(newExpArgs[1] is ConditionalExpression);
        }

    }
}
