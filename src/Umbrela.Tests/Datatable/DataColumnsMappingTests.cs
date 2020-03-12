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
    public class DataColumnsMappingTests
    {
        private readonly List<Person> _people = new List<Person>();


        [Fact(DisplayName = "When projects to an anonymous type, it should generate columns based on the properties projected.")]
        public void ToDataTable_ProjectToAnonymousType_ShouldGenerateDataTableColumnsBasedOnTheProjectionProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => new { ID = p.Id, p.FirstName, p.LastName, DOB = p.DateOfBirth };

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange( new DataColumn[] {
                new DataColumn() { ColumnName = "ID", DataType = typeof(Person).GetProperty("Id").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "LastName", DataType = typeof(Person).GetProperty("LastName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "DOB", DataType = typeof(Person).GetProperty("DateOfBirth").PropertyType, AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When projects to an user defined type, it should generate columns based on the members initialized.")]
        public void ToDataTable_ProjectToUserDefinedType_ShouldGenerateDataTableColumnsWhereItsColumnsArePropertiesInitialized()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id, IsAlive = p.IsAlive };

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "IsAlive", DataType = typeof(Person).GetProperty("IsAlive").PropertyType, AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an implicit projection of an user defined type, it should generate columns based on the built-in type and writable properties.")]
        public void ToDataTable_ProjectToParameter_ShouldImplicitilyGenerateADataTableColumnsBasedOnParameterTypeProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => p;

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "LastName", DataType = typeof(Person).GetProperty("LastName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "DateOfBirth", DataType = typeof(Person).GetProperty("DateOfBirth").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "IsAlive", DataType = typeof(Person).GetProperty("IsAlive").PropertyType, AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that only has a member access without a new operator, it should generate a column based on the member accessed.")]
        public void ToDataTable_ProjectToAMemberAccess_ShouldGenerateColumnBasedOnTheMemberAccessed()
        {
            Expression<Func<Person, dynamic>> projector = p => p.FirstName;

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, AllowDBNull = true }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that has a column settings in it (without a new operator), it should generate a column based on the settings passed within the projection.")]
        public void ToDataTable_ProjectUsingColumnSettings_ShouldGenerateColumnBasedOnSettingsPassedInTheProjection()
        {
            Expression<Func<Person, dynamic>> projector = p => ColumnSettings.Build(() => p.FirstName + " " + p.LastName).Name("Full Name");

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Full Name", DataType = typeof(string), AllowDBNull = true }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection of an anomyous type where one of the properties is nullable, it should generate all columns based on the projection and consider the nullable property when mapping to a column.")]
        public void ToDataTable_ProjectToAnonymousTypeWhereAPropertyIsNullable_ShouldGenerateColumnsBasedOnTheProjection()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Id = p.Id as int? };

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(int), AllowDBNull = true }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an projection that has multiple member accesses combined with an operator, it should thrown an exception due to it can infer the column's name.")]
        public void ToDataTable_ProjectDifferentMemberAccessesWithoutANewOperator_ShouldThrowAnInvalidProjectionException()
        {
            // Members accessed: FirstName and LastName
            Expression<Func<Person, string>> projector = p => p.FirstName + " " + p.LastName;

            Func<DataTable> toDataTable = () => _people.ToDataTable(projector);

            Assert.Throws<InvalidProjectionException>(toDataTable);
        }

        [Fact(DisplayName = "When projects a string member, it should generate a nullable column.")]
        public void ToDataTable_ProjectAStringMember_ShouldGenerateANullableColumn()
        {
            Expression<Func<Person, string>> projector = p => p.FirstName;

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, AllowDBNull = true }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When the projection only has a chain of member access, it should generate a column based on the lattest member accessed.")]
        public void ToDataTable_ProjectAChainOfMemberAccess_ShouldGenerateAColumnBasedOnTheLattestMemberAccessed()
        {
            // Chain of member access: SSN.Id
            Expression<Func<Person, string>> projector = p => p.SSN.Id;

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(string), AllowDBNull = true }
                }
            );
        }

        private static bool AreColumnsSetEquals(DataColumnCollection columnsUnderTest, DataColumnCollection expectedColumns)
        {
            for (var index = 0; index < expectedColumns.Count; index++)
                if (!AreColumnEquals(columnsUnderTest[index], expectedColumns[index]))
                    return false;

            return true;
        }

        private static bool AreColumnEquals(DataColumn firstColumn, DataColumn secondColumn)
        {
            return firstColumn.ColumnName == secondColumn.ColumnName &&
                firstColumn.DataType == secondColumn.DataType &&
                firstColumn.AllowDBNull == secondColumn.AllowDBNull;
        }
    }
}
