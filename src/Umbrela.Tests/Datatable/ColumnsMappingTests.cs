using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Tests;
using Umbrella.Tests.Mocks;
using Xunit;
using System.Linq;
using Umbrella.Exceptions;

namespace Umbrella.Tests.Datatable
{
    public class ColumnsMappingTests
    {

        [Fact(DisplayName = "When projects to an anonymous type, it should generate columns based on the properties projected.")]
        public void GetColumns_ProjectToAnonymousType_ShouldGenerateDataTableColumnsBasedOnTheProjectionProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => new { ID = p.Id, p.FirstName, p.LastName, DOB = p.DateOfBirth };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "ID", DataType = typeof(Person).GetProperty("Id").PropertyType, IsNullable = false},
                new Column(){Name = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, IsNullable = false},
                new Column(){Name = "LastName", DataType = typeof(Person).GetProperty("LastName").PropertyType, IsNullable = false},
                new Column(){Name = "DOB", DataType = typeof(Person).GetProperty("DateOfBirth").PropertyType, IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When projects to an user defined type, it should generate columns based on the members initialized.")]
        public void GetColumns_ProjectToUserDefinedType_ShouldGenerateDataTableColumnsWhereItsColumnsArePropertiesInitialized()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id, IsAlive = p.IsAlive };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, IsNullable = false},
                new Column(){Name = "IsAlive", DataType = typeof(Person).GetProperty("IsAlive").PropertyType, IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an implicit projection of an user defined type, it should generate columns based on the built-in type and writable properties.")]
        public void GetColumns_ProjectToParameter_ShouldImplicitilyGenerateADataTableColumnsBasedOnParameterTypeProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => p;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, IsNullable = false},
                new Column(){Name = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, IsNullable = false},
                new Column(){Name = "LastName", DataType = typeof(Person).GetProperty("LastName").PropertyType, IsNullable = false},
                new Column(){Name = "DateOfBirth", DataType = typeof(Person).GetProperty("DateOfBirth").PropertyType, IsNullable = false},
                new Column(){Name = "IsAlive", DataType = typeof(Person).GetProperty("IsAlive").PropertyType, IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that only has a member access without a new operator, it should generate a column based on the member accessed.")]
        public void GetColumns_ProjectToAMemberAccess_ShouldGenerateColumnBasedOnTheMemberAccessed()
        {
            Expression<Func<Person, dynamic>> projector = p => p.FirstName;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that has a column settings in it (without a new operator), it should generate a column based on the settings passed within the projection.")]
        public void GetColumns_ProjectUsingColumnSettings_ShouldGenerateColumnBasedOnSettingsPassedInTheProjection()
        {
            Expression<Func<Person, dynamic>> projector = p => ColumnSettings.Build(() => p.FirstName + " " + p.LastName).Name("Full Name");

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "Full Name", DataType = typeof(string), IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection of an anomyous type where one of the properties is nullable, it should generate all columns based on the projection and consider the nullable property when mapping to a column.")]
        public void GetColumns_ProjectToAnonymousTypeWhereAPropertyIsNullable_ShouldGenerateColumnsBasedOnTheProjection()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = p.Id as int? };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "Id", DataType = typeof(int), IsNullable = true}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that has multiple member accesses combined with an operator, it should thrown an exception due to it can infer the column's name.")]
        public void GetColumns_ProjectDifferentMemberAccessesWithoutANewOperator_ShouldThrowAnInvalidProjectionException()
        {
            // Members accessed: FirstName and LastName
            Expression<Func<Person, string>> projector = p => p.FirstName + " " + p.LastName;

            Func<List<Column>> getColumns = () => new ColumnsMapping(projector).GetColumns();

            Assert.Throws<InvalidProjectionException>(getColumns);
        }

        [Fact(DisplayName = "When the projection only has a chain of member access, it should generate a column based on the lattest member accessed.")]
        public void GetColumns_ProjectAChainOfMemberAccess_ShouldGenerateAColumnBasedOnTheLattestMemberAccessed()
        {
            // Chain of member access: SSN.Id
            Expression<Func<Person, string>> projector = p => p.SSN.Id;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            var expectedColumns = new List<Column>()
            {
                new Column(){Name = "Id", DataType = typeof(string), IsNullable = false}
            };
            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        private static bool AreColumnsSetEquals(List<Column> columnsUnderTest, List<Column> expectedColumns)
        {
            foreach (var columnTest in columnsUnderTest.Zip(expectedColumns, (columnUnderTest, expectedColumn) => (columnUnderTest, expectedColumn)))
            {
                if (!AreColumnEquals(columnTest.columnUnderTest, columnTest.expectedColumn))
                    return false;
            }

            return true;
        }

        private static bool AreColumnEquals(Column firstColumn, Column secondColumn)
        {
            return firstColumn.Name == secondColumn.Name && 
                firstColumn.DataType == secondColumn.DataType && 
                firstColumn.IsNullable == secondColumn.IsNullable;
        }
    }
}
