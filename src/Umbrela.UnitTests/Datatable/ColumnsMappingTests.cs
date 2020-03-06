using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Umbrella.Tests;
using Umbrella.Tests.Mocks;

namespace Umbrella.Tests.Datatable
{
    [TestClass]
    public class ColumnsMappingTests
    {
        private ColumnsMapping _columnsMapped;

        [TestInitialize]
        public void Init()
        {
            _columnsMapped = null;
        }

        [TestMethod]
        public void ToDataTable_ProjectToAnonymousType_ShouldGenerateDataTableColumnsBasedOnTheProjectionProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => new { ID = p.Id, p.FirstName, p.LastName, DOB = p.DateOfBirth };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.IsTrue(columns.HasAllColumns("ID", "FirstName", "LastName", "DOB"));
        }

        [TestMethod]
        public void ToDataTable_ProjectToUserDefinedType_ShouldGenerateDataTableColumnsWhereItsColumnsArePropertiesInitialized()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id, IsAlive = p.IsAlive };

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            columns.HasAllColumns("Id", "IsAlive");
        }

        [TestMethod]
        public void ToDataTable_ProjectToParameter_ShouldImplicitilyGenerateADataTableColumnsBasedOnParameterTypeProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => p;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.IsTrue(columns.HasAllColumns("Id", "FirstName", "LastName", "IsAlive", "DateOfBirth"));
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
