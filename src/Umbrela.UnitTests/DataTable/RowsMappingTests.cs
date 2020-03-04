using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Umbrella.Tests;
using Umbrella.Tests.Mocks;
using static Umbrella.Tests.Datatable.RowsMappingHelper;

namespace Umbrella.Tests.Datatable
{
    [TestClass]
    public class RowsMappingTests
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
        public void ToDataTable_ProjectToAnonymousTypeThatHasAPropertyBindedToAStringConcat_ShouldGenerateADtWhereTheValuesAllocatedForThatColumnMustHaveTheDataBasedOnTheProjector()
        {
            Expression<Func<Person, dynamic>> projector = p => new {FullName = p.FirstName + " " + p.LastName};

            DataTable dataTable = _people.ToDataTable(projector);
            
            Assert.IsTrue(
                ValidateRowsAreMapped(_people.Select(p => new { FullName = p.FirstName + " " + p.LastName }), dataTable, (p, r) => p.FullName == (string)r["FullName"])
                );
        }

        [TestMethod]
        public void ToDataTable_ProjectToAnUserDefinedType_ShouldGenerateADtWhereTheRowsValuesAreEqualToTheInputCollection()
        {
            Expression<Func<Person, dynamic>> projector = p => new Person() { DateOfBirth = p.DateOfBirth };

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.IsTrue(ValidateRowsAreMapped(_people, dataTable, (p, r) => p.DateOfBirth == (DateTime)r["DateOfBirth"]));
        }

        [TestMethod]
        public void ToDataTable_PassAStaticMethodInTheProjector_ShouldRunLocallyThatMethodAndTreatItsResultAsAConstant()
        {
            Expression<Func<Person, dynamic>> projector = p => new { p.Id, BornPlace = Place.GetPlace()};

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.IsTrue(
                ValidateRowsAreMapped(_people.Select(p => new { p.Id, BornPlace = Place.GetPlace() }), dataTable, (p, r) => p.BornPlace == (string)r["BornPlace"])
            );
        }

        [TestMethod]
        public void ToDataTable_PassAnInstanceMethodInTheProjector_ShouldRunLocallyTheMethodAndTreatItsResultAsAConstant()
        {
            var place = new Place();
            Expression<Func<Person, dynamic>> projector = p => new { BornPlace = place.GetPlace(false), p.IsAlive};

            DataTable dataTable = _people.ToDataTable(projector);

            Assert.IsTrue(
                ValidateRowsAreMapped(_people.Select(p => new { BornPlace = place.GetPlace(false)}), dataTable, (p, r) => p.BornPlace == (string)r["BornPlace"])
                );
        }

        //[TestMethod]
        //public void ToDataTable_PassAConstantAsProjector_ShouldThrowAnExceptionBecauseTheProjectorIsNotReferencingAnyInputTypeProperty()
        //{
        //    var place = new Place();
        //    Expression<Func<Person, dynamic>> projector = p => new { Place = place.GetPlace(true) };

        //    Func<Datatable> toDataTable = () => _people.ToDataTable(projector);

        //    Assert.ThrowsException<InvalidOperationException>(toDataTable);
        //}

        //[TestMethod]
        //public void ToDataTable_PassAProjectorThatHasNestedInstantiationInIt_ShouldThrowAnExceptionBecauseNestedObjectsInAProjectorIsInvalid()
        //{
        //    Expression<Func<Person, dynamic>> projector = p => new { p.Id, OuterObj = new { p.IsAlive } };

        //    Func<Datatable> toDataTable = () => _people.ToDataTable(projector);

        //    Assert.ThrowsException<InvalidOperationException>(toDataTable);
        //}
    }

    public static class RowsMappingHelper
    {
        public static bool ValidateRowsAreMapped<T>(IEnumerable<T> collection, System.Data.DataTable dataTable, Func<T, DataRow, bool> validatorFn)
        {
            int rowIndex = 0;
            foreach (T item in collection)
            {
                DataRow row = dataTable.Rows[rowIndex];

                if (!validatorFn(item, row))
                    return false;
                    //throw new InvalidRowMappingException("The DataTable rows are not matching the input Collection.", item, (dataTable.Columns.ToList(), row));


                rowIndex++;
            }
            
            return true;
        }

    }

    //public class InvalidRowMappingException: Exception
    //{
    //    public string ObjectSerialized { get; private set; }
    //    public string RowSerialized { get; private set; }

    //    public InvalidRowMappingException(string message, object obj, (List<DataColumn>, DataRow) row): base(message)
    //    {
    //        ObjectSerialized = JsonConvert.SerializeObject(obj);
    //        RowSerialized = SerializeRow(row);
    //    }

    //    public static string SerializeRow((List<DataColumn>, DataRow) row)
    //    {
    //        Dictionary<string, object> rowDic = new Dictionary<string, object>();

    //        foreach (DataColumn c in row.Item1)
    //        {
    //            rowDic.Add(c.ColumnName, row.Item2[c]);
    //        }

    //        return JsonConvert.SerializeObject(rowDic);
    //    }
    //}
}