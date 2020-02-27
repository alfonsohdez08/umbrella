using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.UnitTests
{
    [TestClass]
    public class DataTableTests
    {
        private IEnumerable<Person> _people;

        [TestInitialize]
        public void Init()
        {
            _people = new List<Person>()
            {
                new Person(){Id = 1, FirstName = "Juan", LastName = "Doe", DateOfBirth = new DateTime(1990, 01, 22), IsAlive = false},
                new Person(){Id = 2, FirstName = "Jessica", LastName = "Princeton", DateOfBirth = new DateTime(2001, 7, 16), IsAlive = true},
                new Person(){Id = 3, FirstName = "John", LastName = "Bermont", DateOfBirth = new DateTime(1910, 5, 1), IsAlive = false},
                new Person(){Id = 4, FirstName = "Mercedes", LastName = "Johnson", DateOfBirth = new DateTime(1955, 11, 23), IsAlive = true},
            };
        }

        [TestMethod]
        public void ToDataTable_ProjectAnAnonymousType_ShouldGenerateADTWhereItsColumnsAreAnonymousTypeProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => new { ID = p.Id, p.FirstName, p.LastName, DOB = p.DateOfBirth };

            DataTable peopleDataTable = _people.ToDataTable(projector);

            Assert.IsTrue(peopleDataTable.HasAllColumns("ID", "FirstName", "LastName", "DOB"));
        }

        [TestMethod]
        public void ToDataTable_ProjectToAnUserDefinedType_ShouldGenerateADTWhereItsColumnsArePropertiesInitialized()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { Id = p.Id, IsAlive = p.IsAlive };

            DataTable peopleDataTable = _people.ToDataTable(projector);

            Assert.IsTrue(peopleDataTable.HasAllColumns("Id", "IsAlive"));
        }

        [TestMethod]
        public void ToDataTable_ProjectToParameter_ShouldImplicitilyGenerateADtBasedOnParameterTypeProperties()
        {
            Expression<Func<Person, dynamic>> projector = p => p;

            DataTable peopleDataTable = _people.ToDataTable(projector);

            Assert.IsTrue(peopleDataTable.HasAllColumns("Id", "FirstName", "LastName", "IsAlive", "DOB", "DateOfBirth"));
        }

        [TestMethod]
        public void ToDataTable_ProjectToAPrimitiveType_ShouldThrowAnExceptionSayingThatTheProjectorIsInvalid()
        {
            List<int> ids = new List<int>() { 1, 2, 3 };
            Expression<Func<int, dynamic>> projector = i => i;

            Func<DataTable> toDataTable = () => ids.ToDataTable(projector);

            Assert.ThrowsException<ArgumentException>(toDataTable);
        }
    }

    public static class DataTableExtensions
    {
        public static bool HasAllColumns(this DataTable dataTable, params string[] columns)
        {
            for (int index = 0; index < columns.Length; index++)
            {
                string columnName = columns[index];

                if (!dataTable.Columns.Contains(columnName))
                    return false;
            }

            return true;
        }
    }
}
