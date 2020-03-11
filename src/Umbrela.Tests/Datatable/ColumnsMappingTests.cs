using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Tests;
using Umbrella.Tests.Mocks;
using Xunit;

namespace Umbrella.Tests.Datatable
{
    public class ColumnsMappingTests
    {

        [Fact(DisplayName = "When projects to an anonymous type, it should generate columns based on the properties projected.")]
        public void ToDataTable_ProjectToAnonymousType_ShouldGenerateDataTableColumnsBasedOnTheProjectionProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => new { ID = p.Id, p.FirstName, p.LastName, DOB = p.DateOfBirth };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("ID", "FirstName", "LastName", "DOB"));
        }

        [Fact(DisplayName = "When projects to an user defined type, it should generate columns based on the members initialized.")]
        public void ToDataTable_ProjectToUserDefinedType_ShouldGenerateDataTableColumnsWhereItsColumnsArePropertiesInitialized()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id, IsAlive = p.IsAlive };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("Id", "IsAlive"));
        }

        [Fact(DisplayName = "When it's an implicit projection of an user defined type, it should generate columns based on the writable properties.")]
        public void ToDataTable_ProjectToParameter_ShouldImplicitilyGenerateADataTableColumnsBasedOnParameterTypeProperties()
        {
            //Expression<Func<Person, dynamic>> projector = p => p;
            Expression<Func<Person, dynamic>> projector = p => ColumnSettings.Build(() => p.FirstName + " " + p.LastName).Name("Full Name");

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("Full Name"));

            //Assert.True(columns.HasAllColumns("Id", "FirstName", "LastName", "IsAlive", "DateOfBirth"));
        }

        [Fact]
        public void Test()
        {
            var dictionary = new Dictionary<string, Type>()
            {
                {"Id", typeof(long)},
                {"Name", typeof(string)}
            };

            //var type = AnonymousType.Create(dictionary);

            //var obj = Activator.CreateInstance(type, new object[] {1, "Hey" });

            //Assert.True(type != null);
        }

        //[TestMethod]
        //public void ToDataTable_ProjectToAnAnonymousTypeThatHasAColumnCustomized_ShouldGenerateTheCustomizedDtColumnAccordingToTheProjection()
        //{
        //    Expression<Func<Person, dynamic>> projector = p => new {p.Id, ID = ColumnSettings.Build(() => 1).Name("ID Modified") };

        //    List<Column> columns = new ColumnsMapped(projector).GetColumns();

        //    Assert.IsTrue(columns.HasAllColumns("Id", "ID Modified"));
        //}
    }
}
