using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Exceptions;
using Umbrella.Expr.Projection;
using Umbrella.Tests.Mocks;
using Xunit;

namespace Umbrella.Tests.Expr
{
    public class ProjectionValidatorTests
    {
        private ProjectionValidator _projectionValidator = new ProjectionValidator();


        [Fact(DisplayName = "When it's a flat projection, it should not encounter any issue/exception.")]
        public void Validate_FlatProjection_ShouldNotThrowAnException()
        {
            Expression<Func<Person, dynamic>> projector = p => new {p.Id, p.Age};

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            CustomAsserts.NotThrowsException<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection that do not reference the projector's parameter, it should throw an InvalidProjectionException.")]
        public void Validate_AnonymousTypeProjectionThatDoNotReferenceProjectorParameter_ShouldThrowAnInvalidProjectionException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = 1, Age = 31 };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's not a flat projection, it should throw an InvalidProjectionException.")]
        public void Validate_NestedProjections_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = new { p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's not a flat projection and the inner projection's type is a user defined type, it should throw an InvalidProjectionException.")]
        public void Validate_NestedProjectionsWhereTheInnerProjectionIsAnUserDefinedType_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { NewPerson = new Person() { Id = p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection where each property's type is a primitive type, it should not throw an exception.")]
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
                throw;
                //Assert.Fail($"An exception was thrown when executing the given action: {ex.ToString()}");
            }
        }
    }
}
