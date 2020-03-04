using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Expr.Column;
using Umbrella.Expr.UnitTests.Helpers;
using Umbrella.UnitTests;

namespace Umbrella.Expr.UnitTests
{
    [TestClass]
    public class ColumnExpressionMapperTests
    {
        private ColumnExpressionMapper _columnExpressionMapper;

        [TestInitialize]
        public void Init()
        {
            _columnExpressionMapper = new ColumnExpressionMapper();
        }

        [TestMethod]
        public void Map_AnonymousTypeProjectionWithOneProperty_ShouldMapThePropertyToAColumnExpression()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            // Assert
            var memberExpression = (MemberExpression)((NewExpression)projector.Body).GetExpressionByMember("Id");
            var columnExpression = new ColumnExpression(memberExpression);

            var columnExpressionMapped = (ColumnExpression)((NewExpression)projectorMapped).Arguments[0];

            Assert.IsTrue(columnExpression.ColumnDefinition == columnExpressionMapped.ColumnDefinition);
        }

        [TestMethod]
        public void Map_AnonymousTypeProjectionWithMultipleProperties_ShouldMapThePropertiesToColumnExpressions()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, p.FirstName, p.LastName };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            // Asserts 
            Expression[] newExpArgs = ((NewExpression)projector.Body).GetArguments();
            ColumnExpression[] columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped).ToArray();

            if (newExpArgs.Length != columnExpressions.Length)
                Assert.Fail($"Expected {newExpArgs.Length} columns mapped, but instead {columnExpressions.Length} columns were mapped.");

            for (int index = 0; index < newExpArgs.Length; index++)
            {
                Assert.IsTrue(newExpArgs[index] == columnExpressions[index].ColumnDefinition, $"Dismatch between expressions mapped. Expected: {newExpArgs[index]} ; Mapped: {columnExpressions[index].ColumnDefinition}");
            }
        }

        [TestMethod]
        public void Map_UserDefinedTypeProjectionWithOneProperty_ShouldMapThePropertyToColumnExpression()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            // Assert
            var m = (MemberExpression)((MemberInitExpression)projector.Body).GetExpressionByMember("Id");
            var columnExpression = new ColumnExpression(m);

            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.IsTrue(columnExpression.ColumnDefinition == columnExpressions[0].ColumnDefinition);
        }

        [TestMethod]
        public void Map_UserDefinedTypeProjectionWithMultipleProperties_ShouldMapThePropertiesToColumnExpressions()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new Person() {Id = p.Id, FirstName = p.LastName };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            // Asserts 
            Expression[] bindingExpressions = ((MemberInitExpression)projector.Body).GetBindingExpressions();
            ColumnExpression[] columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped).ToArray();

            if (bindingExpressions.Length != columnExpressions.Length)
                Assert.Fail($"Expected {bindingExpressions.Length} columns mapped, but instead {columnExpressions.Length} columns were mapped.");

            for (int index = 0; index < bindingExpressions.Length; index++)
            {
                Assert.IsTrue(bindingExpressions[index] == columnExpressions[index].ColumnDefinition, $"Dismatch between expressions mapped. Expected: {bindingExpressions[index]} ; Mapped: {columnExpressions[index].ColumnDefinition}");
            }
        }

        [TestMethod]
        public void Map_AnEmptyProjection_ShouldNotMapAnythingBecauseItsNotProjectingProperties()
        {
            // Arrange
            Expression<Func<Person, dynamic>> projector = p => new { };

            // Act
            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            // Assert
            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.IsTrue(columnExpressions.Count == 0);
        }

        [TestMethod]
        public void Map_PassAMemberAcessAsProjection_ShouldMapToColumnExpression()
        {
            Expression<Func<Person, dynamic>> projector = p => p.Id;

            Expression projectorMapped = _columnExpressionMapper.Map(projector.Body);

            var memberExpression = (MemberExpression)projector.Body;
            var columnExpression = new ColumnExpression(memberExpression);

            List<ColumnExpression> columnExpressions = new ColumnExpressionsFetcher().FetchAll(projectorMapped);

            Assert.IsTrue(columnExpression.ColumnDefinition == columnExpressions[0].ColumnDefinition);
        }
    }
}
