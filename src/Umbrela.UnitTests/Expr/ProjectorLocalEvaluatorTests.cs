using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Projector;
using Umbrella.Tests.Mocks;

namespace Umbrella.Tests.Expr
{
    [TestClass]
    public class ProjectorLocalEvaluatorTests
    {
        private ProjectorLocalEvaluator _localEvaluator;

        [TestInitialize]
        public void Init()
        {
            _localEvaluator = new ProjectorLocalEvaluator();
        }

        [TestMethod]
        public void Evaluate_PassAnInstanceMethodInTheProjectorThatReferenceTheProjectoParameter_ShouldNotExecuteTheMethodLocally()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { PersonId = p.Id, Taxes = new TaxService().GetTaxes(p.Id)};

            // Act
            Expression projectorEvaluated = _localEvaluator.Evaluate(projector.Body, projector.Parameters[0]);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated).GetArguments();

            Assert.IsTrue(newExpArgs[1].NodeType == ExpressionType.Call);
        }

        [TestMethod]
        public void Evaluate_PassAStaticMethodInTheProjectorThatDoNotReferenceTheProjectoParameter_ShouldExecuteTheMethodAndTreatItsResultAsAConstant()
        {
            Expression<Func<Person, dynamic>> projector = p => new { PersonId = p.Id, TaxCounselor = TaxService.GetClosestTaxCounselor() };

            Expression projectorEvaluated = _localEvaluator.Evaluate(projector.Body, projector.Parameters[0]);

            Expression[] newExpArgs = ((NewExpression)projectorEvaluated).GetArguments();

            Assert.IsTrue(newExpArgs[1] is ConstantExpression constantExp && (string)constantExp.Value == TaxService.GetClosestTaxCounselor());
        }

        [TestMethod]
        public void Evaluate_PassAnInlineConditionThatDoNotReferenceTheProjectorParameter_ShouldExecuteItLocallyAndTreatItAsAConstant()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, CanGetIncomingTax = new TaxService().IsIncomingTaxSeason() ? true : false};

            // Act
            Expression projectorEvaluated = _localEvaluator.Evaluate(projector.Body, projector.Parameters[0]);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated).GetArguments();

            bool canGetIncomingTax = new TaxService().IsIncomingTaxSeason() ? true : false;
            Assert.IsTrue(newExpArgs[1] is ConstantExpression constantExp && (bool)constantExp.Value == canGetIncomingTax);
        }

        [TestMethod]
        public void Evaluate_PassAnInlineConditionThatReferenceTheProjectorParameter_ShouldNotExecuteItLocally()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, CanGetIncomingTax = TaxService.IsTaxAvailable(p.Id) ? true : false };

            // Act
            Expression projectorEvaluated = _localEvaluator.Evaluate(projector.Body, projector.Parameters[0]);

            // Assert
            Expression[] newExpArgs = ((NewExpression)projectorEvaluated).GetArguments();

            Assert.IsTrue(newExpArgs[1] is ConditionalExpression);
        }
    }
}
