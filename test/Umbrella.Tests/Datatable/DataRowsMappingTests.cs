using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Umbrella.Tests;
using Umbrella.Tests.Stubs;
using Xunit;
using Umbrella.Expr.Evaluators;

namespace Umbrella.Tests.Datatable
{
    public class DataRowsMappingTests
    {
        private readonly IEnumerable<Person> _people;

        public DataRowsMappingTests()
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
        public void ToDataTable_ProjectionThatHasAPropertyMappedToACompositeExpression_ShouldEvaluateTheCompoundExpressionAndDumpTheDataAccordingToTheExpression()
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

        [Fact(DisplayName = "When projects to an user defined type, it should evaluate each expression assigned to the member initialized.")]
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
        public void ToDataTable_ProjectAString_ShouldSetDBNullWheneverFindsANull()
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

        [Fact(DisplayName = "When there's an instance method call that do not reference the projector's parameter within the projection, it should execute the expression with no issue.")]
        public void ToDataTable_OneOfTheProjectedPropertiesCallsAnInstanceMethodThatDoNotReferenceTheProjectorParameter_ShouldEvaluateTheMappingExpression()
        {
            Expression<Func<Person, dynamic>> projector =
                p => new { Name = p.FirstName + " " + p.LastName, IsGettingTaxes = new TaxService().IsIncomingTaxSeason() };

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(AreDataSetEquals(
                dataTable,
                _people.Select(projector.Compile()).ToList(),
                (d, p) => p.Name == (string)d["Name"] && p.IsGettingTaxes == (bool)d["IsGettingTaxes"]
                )
            );
        }

        [Fact(DisplayName = "When there's an instance method call that references the projector's parameter within the projection, it should execute the expression with no issue.")]
        public void ToDataTable_OneOfTheProjectedPropertiesCallsAnInstanceMethodThatReferencesTheProjectorParameter_ShouldEvaluateTheMappingExpression()
        {
            Expression<Func<Person, dynamic>> projector =
                p => new { Name = p.FirstName + " " + p.LastName, TaxAmount = new TaxService().GetTaxes(p.Id) };

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.True(AreDataSetEquals(
                dataTable,
                _people.Select(projector.Compile()).ToList(),
                (d, p) => p.Name == (string)d["Name"] && p.TaxAmount == (decimal)d["TaxAmount"]
                )
            );
        }

        [Fact(DisplayName = "When the projection has a column settings in it, it should take out the mapper expression from the column setting and set it as column mapper expression.")]
        public void ToDataTable_ProjectionThatHasColumnSettingsInIt_ShouldTakeTheMapperExpressionFromTheColumnSettingAndEvaluateIt()
        {
            // TODO: for fun, inspect this using ILSpy
            Expression<Func<Person, dynamic>> projector =
                p => new { Name = ColumnSettings.Build(() => p.FirstName + " " + p.LastName).Name("F. Name"), IsGettingTaxes = new TaxService().IsIncomingTaxSeason() };

            DataTable dataTable = _people.ToDataTable(projector);

            ParameterExpression projectorParameter = projector.Parameters[0];
            Assert.True(AreDataSetEquals(
                dataTable,
                _people.Select(projector.Compile()).ToList(), // when compiles the lambda expression, all the projector parameter references are closured
                (d, p) => (string)ExecuteMapper(p.Name.Mapper) == (string)d["F. Name"] && 
                    p.IsGettingTaxes == (bool)d["IsGettingTaxes"]
                )
            );
        }

        [Fact(DisplayName = "When a projection receives a primitive and output an object, it should dump the data based on the expressions declared within the projection.")]
        public void ToDatatable_ProjectorThatHasAsInputAPrimitiveAndProjectsAnObject_ShouldDumpTheDataBasedOnTheExpressionsSetWithinTheProjection()
        {
            var ids = new List<long>() { 1, 2, 3, 4, 5 };

            Expression<Func<long, dynamic>> projector = l => new { Id = l };

            DataTable dataTable = ids.ToDataTable(projector);

            Assert.True(
                AreDataSetEquals(
                    dataTable,
                    ids.Select(projector.Compile()).ToList(),
                    (d, l) => (long)d["Id"] == l.Id
                )
            );
        }

        private static bool AreDataSetEquals<T>(DataTable dataTable, List<T> expectedData, Func<DataRow, T, bool> areEqual)
        {
            foreach (var dataMappingTest in dataTable.Select().Zip(expectedData, (dataRow, element) => (dataRow, element)))
                if (!areEqual(dataMappingTest.dataRow, dataMappingTest.element))
                    return false;

            return true;
        }

        private static bool AreDataSetEquals(DataTable dataTable, List<dynamic> expectedData, Func<DataRow, dynamic, bool> areEqual)
        {
            foreach (var dataMappingTest in dataTable.Select().Zip(expectedData, (dataRow, element) => (dataRow, element)))
                if (!areEqual(dataMappingTest.dataRow, dataMappingTest.element))
                    return false;

            return true;
        }

        private static object ExecuteMapper(Expression mapper)
        {
            var mapperLambdaExp = (LambdaExpression)mapper;
            Delegate mapperDelegate = mapperLambdaExp.Compile();

            return mapperDelegate.DynamicInvoke();
        }

    }
}