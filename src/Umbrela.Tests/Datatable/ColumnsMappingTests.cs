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
            Expression<Func<Person, dynamic>> projector = p => p;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("Id", "FirstName", "LastName", "IsAlive", "DateOfBirth"));
        }

        [Fact(DisplayName = "When it's an projection that has only a member access without a new operator, it should generate a column based on the member accessed.")]
        public void GetColumns_ProjectToAMemberAccess_ShouldGenerateColumnBasedOnTheMemberAccessed()
        {
            Expression<Func<Person, dynamic>> projector = p => p.FirstName;

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("FirstName"));
        }

        [Fact(DisplayName = "When it's an projection that has a column settings in it (without a new operator), it should generate a column based on the settings passed within the projection.")]
        public void GetColumns_ProjectUsingColumnSettings_ShouldGenerateColumnBasedOnSettingsPassedInTheProjection()
        {
            Expression<Func<Person, dynamic>> projector = p => ColumnSettings.Build(() => p.FirstName + " " + p.LastName).Name("Full Name");

            List<Column> columns = new ColumnsMapping(projector).GetColumns();

            Assert.True(columns.HasAllColumns("Full Name"));
        }
    }
}
