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


        [Fact(DisplayName = "When it's a flat projection, it should not throw validation exception.")]
        public void Validate_FlatProjection_ShouldNotThrowAnException()
        {
            Expression<Func<Person, dynamic>> projector = p => new {p.Id, p.Age};

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            CustomAsserts.NotThrowException<Exception>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection that do not reference the projector's parameter, it should throw an exception.")]
        public void Validate_AnonymousTypeProjectionThatDoNotReferenceProjectorParameter_ShouldThrowAnInvalidProjectionException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = 1, Age = 31 };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's not a flat projection, it should throw an exception.")]
        public void Validate_NestedProjections_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = new { p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a nested projection, it should throw an exception.")]
        public void Validate_NestedProjectionsWhereTheInnerProjectionIsAnUserDefinedType_ShouldThrowAnInvalidOperationException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { NewPerson = new Person() { Id = p.Id }, p.Age };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            Assert.Throws<InvalidProjectionException>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection that takes a primitive type and wrap it with a new operator, it should not throw an exception.")]
        public void Validate_ProjectAPrimitiveWrappedByANewOperator_ShouldNotThrowAnException()
        {
            Expression<Func<int, dynamic>> projector = i => new { Id = i };

            Action projectorValidation = () => _projectionValidator.Validate(projector);

            CustomAsserts.NotThrowException<Exception>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection has a single ColumnSettings with a name set, it should not throw an exception.")]
        public void Validate_SingleMemberProjectionWhereColumnSettingsHaveANameSet_ShouldNotThrowAnException()
        {
            Expression<Func<Place, dynamic>> projector = p => ColumnSettings.Build(() => "Hey").Name("HEY");
            var columnSettings = (ColumnSettings)Expression.Lambda(projector.Body).Compile().DynamicInvoke();
            projector = projector.Update(Expression.Constant(columnSettings, typeof(ColumnSettings)), projector.Parameters);

            Action projectorValidation = () => new ColumnSettingsNameValidator().Validate(projector);

            CustomAsserts.NotThrowException<Exception>(projectorValidation);
        }

        [Fact(DisplayName = "When it's a projection has a single ColumnSettings without a name set, it should throw an exception.")]
        public void Validate_SingleMemberProjectionWhereColumnSettingsDoNotHaveANameSet_ShouldThrowAnException()
        {
            Expression<Func<Place, dynamic>> projector = p => ColumnSettings.Build(() => "Hey");
            var columnSettings = (ColumnSettings)Expression.Lambda(projector.Body).Compile().DynamicInvoke();
            projector = projector.Update(Expression.Constant(columnSettings, typeof(ColumnSettings)), projector.Parameters);

            Action projectorValidation = () => new ColumnSettingsNameValidator().Validate(projector);

            Assert.Throws<InvalidOperationException>(projectorValidation);
        }

    }


    public static class CustomAsserts
    {
        public static void NotThrowException<TException>(Action action) where TException: Exception
        {
            try
            {
                action();
            }catch(TException e)
            {
                Assert.True(false, $"Failed because an exception of type {typeof(TException).Name} was thrown.");
            }
        }
    }
}
