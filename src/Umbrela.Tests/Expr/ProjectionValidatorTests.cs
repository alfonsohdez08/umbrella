using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;
using Umbrella.Expr.Projector;
using Umbrella.Tests.Mocks;

namespace Umbrella.Tests.Expr
{
    [TestClass]
    public class ProjectionValidatorTests
    {
        private ProjectionValidator _projectionValidator = new ProjectionValidator();


        [TestMethod]
        public void Validate_FlatProjection_ShouldNotThrowAnException()
        {
            Expression<Func<Person, dynamic>> projector = p => new {p.Id, p.Age};

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            CustomAsserts.NotThrowsException<InvalidProjectionException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_AnonymousTypeProjectionThatDoNotReferenceProjectorParameter_ShouldThrowAnInvalidProjectionException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = 1, Age = 31 };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.ThrowsException<InvalidProjectionException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_NestedProjections_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = new { p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.ThrowsException<InvalidProjectionException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_NestedProjectionsWhereTheInnerProjectionIsAnUserDefinedType_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { NewPerson = new Person() { Id = p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.ThrowsException<InvalidProjectionException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_ProjectAPrimitive_ShouldNotThrowAnException()
        {
            Expression<Func<int, dynamic>> projector = i => new { Id = i };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            CustomAsserts.NotThrowsException<Exception>(projectorValidation);
        }
    }

    public static class CustomAsserts
    {
        public static void NotThrowsException<TException>(Action action) where TException: Exception
        {
            try
            {
                action();
            }catch(TException ex)
            {
                Assert.Fail($"An exception was thrown when executing the given action: {ex.ToString()}");
            }
        }
    }
}
