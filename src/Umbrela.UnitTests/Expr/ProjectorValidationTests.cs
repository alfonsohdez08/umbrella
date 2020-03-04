using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Projector;

namespace Umbrella.Tests.Expr
{
    [TestClass]
    public class ProjectorValidationTests
    {

        [TestMethod]
        public void Validate_FlatProjection_ShouldNotThrowAnException()
        {
            Expression<Func<Person, dynamic>> projector = p => new {p.Id, p.Age};

            Action projectorValidation = () => ProjectorValidator.Validate(projector);

            CustomAsserts.NotThrowsException<Exception>(projectorValidation);
        }

        [TestMethod]
        public void Validate_AnonymousTypeProjectionThatDoNotReferenceProjectorParameter_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = 1, Age = 31 };

            Action projectorValidation = () => ProjectorValidator.Validate(projector);

            Assert.ThrowsException<InvalidOperationException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_NestedProjections_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = new { p.Id }, p.Age };

            Action projectorValidation = () => ProjectorValidator.Validate(projector);

            Assert.ThrowsException<InvalidOperationException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_NestedProjectionsWhereTheInnerProjectionIsAnUserDefinedType_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { NewPerson = new Person() { Id = p.Id }, p.Age };

            Action projectorValidation = () => ProjectorValidator.Validate(projector);

            Assert.ThrowsException<InvalidOperationException>(projectorValidation);
        }

        [TestMethod]
        public void Validate_ProjectAPrimitive_ShouldNotThrowAnException()
        {
            Expression<Func<int, dynamic>> projector = i => new { Id = i };

            Action projectorValidation = () => ProjectorValidator.Validate(projector);

            CustomAsserts.NotThrowsException<Exception>(projectorValidation);
        }
    }

    public static class CustomAsserts
    {
        public static void NotThrowsException<T>(Action action) where T: Exception
        {
            try
            {
                action();
            }catch(T ex)
            {
                Assert.Fail($"An exception was thrown when executing the given action: {ex.ToString()}");
            }
        }
    }
}
