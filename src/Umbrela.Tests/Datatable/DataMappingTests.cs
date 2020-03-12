using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Umbrella.Tests;
using Umbrella.Tests.Mocks;
using Xunit;

namespace Umbrella.Tests.Datatable
{
    public class DataMappingTests
    {
        private readonly IEnumerable<Person> _people;

        public DataMappingTests()
        {
            _people = new List<Person>()
            {
                new Person(){Id = 1, FirstName = "Juan", LastName = "Doe", DateOfBirth = new DateTime(1990, 01, 22), IsAlive = false},
                new Person(){Id = 2, FirstName = "Jessica", LastName = "Princeton", DateOfBirth = new DateTime(2001, 7, 16), IsAlive = true},
                new Person(){Id = 3, FirstName = "John", LastName = "Bermont", DateOfBirth = new DateTime(1910, 5, 1), IsAlive = false},
                new Person(){Id = 4, FirstName = "Mercedes", LastName = "Johnson", DateOfBirth = new DateTime(1955, 11, 23), IsAlive = true},
            };
        }

        [Fact(DisplayName = "When a projection of anonomous type that has multiple properties, it should evaluate each mapping expression and dump the data based on them.")]
        public void ToDataTable_ProjectionOfAnonmousTypeThatHasMultipleProperties_ShouldEvaluateTheMappingExpressionsAndDumpTheDataAccordingToEachExpression()
        {
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, p.FirstName, p.IsAlive };

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(AreDataSetEquals(
                dataTable, 
                _people.Select(projector.Compile()).ToList(),
                (d, p) => (int)d["Id"] == p.Id && (string)d["FirstName"] == p.FirstName && (bool)d["IsAlive"] == p.IsAlive)
            );
        }

        [Fact(DisplayName = "When a projection has a property mapped to a compound expression, it should evaluate this compound expression and return the data according to the expression.")]
        public void ToDataTable_ProjectionThatHasAPropertyMappedToACompositeExpression_ShouldEvaluateThisCompoundExpressionAndDumpTheDataAccordingToTheExpression()
        {
            Expression<Func<Person, dynamic>> projector = p => new {FullName = p.FirstName + " " + p.LastName};

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(AreDataSetEquals(
                dataTable,
                _people.Select(projector.Compile()).ToList(),
                (d, p) => (string)d["FullName"] == p.FullName)
            );
        }

        [Fact(DisplayName = "When a projection has a nullable property, it should add DBNull whenever finds a null in the nullable column.")]
        public void ToDataTable_ProjectionThatHasANullableProperty_ShouldSetDBNullValueWheneverFindsANullInTheInputSet()
        {
            var people = _people
                .Select(p => new { p.Id, p.FirstName, p.LastName, IsAlive = p.Id % 2 == 0 ? (bool?)p.IsAlive : null})
                .ToList();

            DataTable dataTable = people.ToDataTable(p => new { p.Id, p.FirstName, p.IsAlive});

            Assert.True(AreDataSetEquals(
                dataTable,
                people,
                (d, p) => (p.IsAlive == null && d["IsAlive"] == DBNull.Value) || (p.IsAlive.HasValue && d["IsAlive"] != DBNull.Value))
            );
        }

        [Fact(DisplayName = "When projects to an user defined type, it should evaluate each expression that are mapped to each member.")]
        public void ToDataTable_ProjectToAnUserDefinedType_ShouldGenerateADtWhereTheRowsValuesAreEqualToTheInputCollection()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { DateOfBirth = p.DateOfBirth };

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(
                AreDataSetEquals(
                    dataTable,
                    _people.Select(projector.Compile()).ToList(),
                    (d, p) => (DateTime)d["DateOfBirth"] == p.DateOfBirth
                )
            );
        }

        [Fact(DisplayName = "When projects a string member, it should mark it as nullable and set DBNull whenever finds a null.")]
        public void ToDataTable_ProjectAString_ItShouldSetDBNullWheneverFindsANull()
        {
            var people = _people.Select(p => new { Name = p.Id % 2 == 0 ? p.FirstName : null }).ToList();

            DataTable dataTable = people.ToDataTable(p => p.Name);

            Assert.True(
                AreDataSetEquals(
                    dataTable,
                    people,
                    (d, p) => (p.Name == null && d["Name"] == DBNull.Value) || (p.Name != null && d["Name"] != DBNull.Value)
                )
            );
        }

        private static bool AreDataSetEquals<T>(DataTable dataTable, List<T> expectedData, Func<DataRow, T, bool> areEqual)
        {
            foreach (var dataMappingTest in dataTable.Select().Zip(expectedData, (dataRow, element) => (dataRow, element)))
            {
                if (!areEqual(dataMappingTest.dataRow, dataMappingTest.element))
                    return false;
            }

            return true;
        }

        private static bool AreDataSetEquals(DataTable dataTable, List<dynamic> expectedData, Func<DataRow, dynamic, bool> areEqual)
        {
            foreach (var dataMappingTest in dataTable.Select().Zip(expectedData, (dataRow, element) => (dataRow, element)))
            {
                if (!areEqual(dataMappingTest.dataRow, dataMappingTest.element))
                    return false;
            }

            return true;
        }

        [Fact]
        public void TestNullability()
        {
            //Expression<Func<Person, dynamic>> projector = p => new { Id = (int?)p.Id, IsAlive = (bool?)p.IsAlive };
            //Expression<Func<Person, dynamic>> projector = p => new {Name = (string?)p.FirstName, Id = (int?)p.Id, IsAlive = (bool?)p.IsAlive };

            //DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(true);
        }
    }
}