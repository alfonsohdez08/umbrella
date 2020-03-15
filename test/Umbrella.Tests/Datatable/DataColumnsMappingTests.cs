using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Tests;
using Umbrella.Tests.Stubs;
using Xunit;
using System.Linq;
using Umbrella.Exceptions;

namespace Umbrella.Tests.Datatable
{
    public class DataColumnsMappingTests
    {
        private readonly List<Person> _people = new List<Person>();


        [Fact(DisplayName = "When projects to an anonymous type, it should generate columns based on the properties projected.")]
        public void ToDataTable_ProjectToAnonymousType_ShouldGenerateDataTableColumnsBasedOnThePropertiesProjected()
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
        public void ToDataTable_ProjectToUserDefinedType_ShouldGenerateDataTableColumnsWhereItsColumnsAreThePropertiesInitialized()
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
        public void ToDataTable_ProjectToUserDefinedTypeParameter_ShouldGenerateDataTableColumnsBasedOntheWritableProperties()
        {
            Expression<Func<Person, Person>> projector = p => p;

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "FirstName", DataType = typeof(Person).GetProperty("FirstName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "LastName", DataType = typeof(Person).GetProperty("LastName").PropertyType, AllowDBNull = true },
                new DataColumn() { ColumnName = "DateOfBirth", DataType = typeof(Person).GetProperty("DateOfBirth").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "IsAlive", DataType = typeof(Person).GetProperty("IsAlive").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "HasChildren", DataType = Nullable.GetUnderlyingType(typeof(Person).GetProperty("HasChildren").PropertyType), AllowDBNull = true }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an implicit projection of an anonymous type, it should generate columns based on the anonymous type properties.")]
        public void ToDataTable_ProjectToAnonymousTypeParameter_ShouldGenerateDataTableColumnsBasedOntheWritableProperties()
        {
            DataColumnCollection columns = _people.Select(p => new { p.Id, p.HasChildren}).ToDataTable(p => p).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(Person).GetProperty("Id").PropertyType, AllowDBNull = false },
                new DataColumn() { ColumnName = "HasChildren", DataType = Nullable.GetUnderlyingType(typeof(Person).GetProperty("HasChildren").PropertyType), AllowDBNull = true }
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

            Func<DataColumnCollection> getColumns = () => _people.ToDataTable(projector).Columns;

            Assert.Throws<InvalidProjectionException>(getColumns);
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

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When the projection does not have a reference to the projector's parameter, it should thrown an exception.")]
        public void ToDataTable_ParameterlessProjection_ShouldThrowAnInvalidProjectionException()
        {
            Expression<Func<Person, dynamic>> projector = p => new { Foo = 1, Description = "hey" };

            Func<DataColumnCollection> getColumns = () => _people.ToDataTable(projector).Columns;

            Assert.Throws<InvalidProjectionException>(getColumns);
        }

        [Fact(DisplayName = "When the projector projects a constant, it should thrown an exception.")]
        public void ToDataTable_ProjectAConstant_ShouldThrowAnInvalidProjectionException()
        {
            Expression<Func<Person, dynamic>> projector = p => "Hey, I am constant!";

            Func<DataColumnCollection> getColumns = () => _people.ToDataTable(projector).Columns;

            Assert.Throws<InvalidProjectionException>(getColumns);
        }

        [Fact(DisplayName = "When the projection has a property whose type is not a .NET built-in type, it should throw an exception.")]
        public void ToDataTable_ProjectionHasANonBuiltInType_ShouldThrowAnInvalidColumnDataTypException()
        {
            var ssn = new SSN();
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, SSN = ssn };

            Func<DataColumnCollection> getColumns = () => _people.ToDataTable(projector).Columns;

            Assert.Throws<InvalidColumnDataTypeException>(getColumns);
        }

        [Fact(DisplayName = "When it's an implicit projection where a couple of properties are nullable, it should generate nullable columns.")]
        public void ToDataTable_ImplicitProjectionThatHasNullableProperties_ShouldGenerateNullableColumns()
        {
            var places = new List<Place>()
            {
                new Place(){PlaceId = 1, AverageIncoming = 10m, IsExpensiveArea = false },
                new Place(){PlaceId = 5, AverageIncoming = null, IsExpensiveArea = false },
                new Place(){PlaceId = null, AverageIncoming = 2000m, IsExpensiveArea = true },
            };
            Expression<Func<Place, Place>> projector = p => p;

            DataColumnCollection columns = places.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "PlaceId", DataType = typeof(int), AllowDBNull = true },
                new DataColumn() { ColumnName = "AverageIncoming", DataType = typeof(decimal), AllowDBNull = true },
                new DataColumn() { ColumnName = "IsExpensiveArea", DataType = typeof(bool), AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's an implicit projection of a struct, it should generate columns based on the writable properties and whose type are built-in.")]
        public void ToDataTable_ImplicitProjectionOfAnStruct_ShouldGenerateColumnsBasedOnTheBuiltInTypeAndWrittableProperties()
        {
            var cars = new List<Car>()
            {
                new Car(){Brand = "Toyota", Model = "Corolla", Year = 2010, IsAvailable = true, PriceInMarket = 2000m},
                new Car(){Brand = "Toyota", Model = "Camry", Year = 2000, IsAvailable = null, PriceInMarket = 200m},
                new Car(){Brand = "Honda", Model = "Civic", Year = 2012, IsAvailable = false, PriceInMarket = 1000m},
            };
            Expression<Func<Car, Car>> projector = c => c;

            DataColumnCollection columns = cars.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Brand", DataType = typeof(string), AllowDBNull = true },
                new DataColumn() { ColumnName = "Model", DataType = typeof(string), AllowDBNull = true },
                new DataColumn() { ColumnName = "Year", DataType = typeof(int), AllowDBNull = false },
                new DataColumn() { ColumnName = "IsAvailable", DataType = typeof(bool), AllowDBNull = true },
                new DataColumn() { ColumnName = "PriceInMarket", DataType = typeof(decimal), AllowDBNull = true },
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When a projection receives a primitive and output an object, it generate columns based on the expressions declared within the projection.")]
        public void ToDatatable_ProjectorThatHasAsInputAPrimitiveAndProjectsAnObject_ShouldGenerateColumnsBasedOnTheExpressionsDeclared()
        {
            var ids = new List<long>() { 1, 2, 3, 4, 5 };
            Expression<Func<long, dynamic>> projector = l => new { Id = l };

            DataColumnCollection columns = ids.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(long), AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's a projection that only has a ColumnSettings in it, it should generate the column with no issue.")]
        public void ToDataTable_ProjectionThatOnlyHasAColumnSettingsInIt_ShouldGenerateTheColumnWithNoIssue()
        {
            Expression<Func<Person, dynamic>> projector = p => new { DOB1 = ColumnSettings.Build(() => p.DateOfBirth).Name("DOB")};

            DataColumnCollection columns = _people.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "DOB", DataType = typeof(DateTime), AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        [Fact(DisplayName = "When it's a single projection that has a ColumnSettings in it, it should generate the column with no issue.")]
        public void ToDataTable_SingleProjectionThatUsesAColumnSettingInIt_ShouldGenerateTheColumnWithNoIssue()
        {
            var ids = new List<int>() { 1, 2 };
            Expression<Func<int, dynamic>> projector = i => ColumnSettings.Build(() => i).Name("Id");

            DataColumnCollection columns = ids.ToDataTable(projector).Columns;

            DataColumnCollection expectedColumns = new DataTable().Columns;
            expectedColumns.AddRange(new DataColumn[] {
                new DataColumn() { ColumnName = "Id", DataType = typeof(int), AllowDBNull = false }
                }
            );

            Assert.True(AreColumnsSetEquals(columns, expectedColumns));
        }

        private static bool AreColumnsSetEquals(DataColumnCollection columnsUnderTest, DataColumnCollection expectedColumns)
        {
            if (columnsUnderTest.Count != expectedColumns.Count)
                return false;

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
