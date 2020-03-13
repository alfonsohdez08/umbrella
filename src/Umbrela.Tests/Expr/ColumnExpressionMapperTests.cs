using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbrella.Expr.Column;
using Umbrella.Tests.Helpers;
using Umbrella.Tests.Mocks;
using Xunit;

namespace Umbrella.Tests.Expr
{
    
    public class ColumnExpressionMapperTests
    {
        private ColumnExpressionMapper _columnExpressionMapper = new ColumnExpressionMapper();

        [Fact(DisplayName = "When projects to an anonmyous type that only has one property, it should mark the property's expression as a column within the projection.")]
        public void Map_AnonymousTypeProjectionWithOneProperty_ShouldMapThePropertyToAColumnExpression()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            // Assert
            var memberExpression = (MemberExpression)((NewExpression)projector.Body).GetExpressionByMember("Id");
            ColumnExpression columnExpressionMapped = new ColumnExpressionsFetcher().FetchAll(projectorMapped)[0];

            Assert.True(columnExpressionMapped.ColumnMapper == memberExpression);
        }

        [Fact(DisplayName = "When projects to an anonymous type that has multiple properties, it should mark all the properties expression as columns within the projection.")]
        public void Map_AnonymousTypeProjectionWithMultipleProperties_ShouldMapThePropertiesToColumnExpressions()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, p.FirstName, p.LastName };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            // Asserts 
            Expression[] newExpArgs = ((NewExpression)projector.Body).GetArguments();
            ColumnExpression[] columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped).ToArray();

            Assert.False(newExpArgs.Length != columnExpressions.Length, $"Expected {newExpArgs.Length} columns mapped, but instead {columnExpressions.Length} columns were mapped.");

            for (int index = 0; index < newExpArgs.Length; index++)
                Assert.True(newExpArgs[index] == columnExpressions[index].ColumnMapper, $"Dismatch between expressions mapped. Expected: {newExpArgs[index]} ; Mapped: {columnExpressions[index].ColumnMapper}");
        }

        [Fact(DisplayName = "When projects to an user defined type that only initialize a member, it should mark the member assignment as a column within the projection.")]
        public void Map_UserDefinedTypeProjectionWithOneProperty_ShouldMapThePropertyToColumnExpression()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            // Assert
            var m = (MemberExpression)((MemberInitExpression)projector.Body).GetExpressionByMember("Id");
            var columnExpression = new ColumnExpression(m);

            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.True(columnExpression.ColumnMapper == columnExpressions[0].ColumnMapper);
        }
        
        [Fact(DisplayName = "When projects to an user defined type with multiple members initialized, it should mark the member assignments as columns within the projection.")]
        public void Map_UserDefinedTypeProjectionWithMultipleProperties_ShouldMapThePropertiesToColumnExpressions()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new Person() {Id = p.Id, FirstName = p.LastName };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            // Asserts 
            Expression[] bindingExpressions = ((MemberInitExpression)projector.Body).GetBindingExpressions();
            ColumnExpression[] columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped).ToArray();

            Assert.False(bindingExpressions.Length != columnExpressions.Length, $"Expected {bindingExpressions.Length} columns mapped, but instead {columnExpressions.Length} columns were mapped.");

            for (int index = 0; index < bindingExpressions.Length; index++)
                Assert.True(bindingExpressions[index] == columnExpressions[index].ColumnMapper, $"Dismatch between expressions mapped. Expected: {bindingExpressions[index]} ; Mapped: {columnExpressions[index].ColumnMapper}");

        }

        [Fact(DisplayName = "When projection has nothing in it, it should not have any column within the projection.")]
        public void Map_AnEmptyProjection_ShouldNotMapAnythingBecauseItsNotProjectingProperties()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            // Assert
            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.True(columnExpressions.Count == 0);
        }

        [Fact(DisplayName = "When projection is a member access, it should mark the member access as a column within the projection.")]
        public void Map_PassAMemberAccessAsProjection_ShouldMapToColumnExpression()
        {
            Expression<Func<Person, int>> projector = p => p.Id;

            Expression projectorMapped = _columnExpressionMapper.Map(projector);

            var memberExpression = (MemberExpression)projector.Body;

            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.True(memberExpression == columnExpressions[0].ColumnMapper);
        }
    }
}
