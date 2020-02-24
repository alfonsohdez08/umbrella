using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Umbrella;


namespace Umbrella.UnitTests
{
    [TestClass]
    public class DataTableConverterTests
    {
        private List<Product> _products;
        
        [TestInitialize]
        public void Init()
        {
            _products = new List<Product>()
            {
                new Product(){Id = 1, Description = "Chocolate", IsAvailable = true, UnitPrice = 1},
                new Product(){Id = 2, Description = "Milk", IsAvailable = false, UnitPrice = 0.5m},
                new Product(){Id = 3, Description = "Cereal", IsAvailable = true, UnitPrice = 2},
            };
        }

        [TestMethod]
        public void ToDataTable_PassAKnownObjectConstructionAsProjector_ShouldGenerateDataTableBasedOnMembersInitialized()
        {
            Expression<Func<Product, Product>> projector = p => new Product() { Id = p.Id, Description = p.Description };

            DataTable dataTable = _products.ToDataTable(projector);

            Assert.IsTrue(dataTable.Columns.Contains("Id"));
            Assert.IsTrue(dataTable.Columns.Contains("Description"));
        }

        [TestMethod]
        public void ToDataTable_PassAnAnonymousProjector_ShouldGenerateDataTableBasedOnTheAnonymousTypeProperties()
        {
            //Expression<Func<Product, object>> projector = p => new { ID = p.Id, p.Description };
            //Expression<Func<Product, object>> projector2 = p =>  p.Id;
            Expression<Func<Product, object>> projector = p => p;

            DataTable dataTable = _products.ToDataTable(projector);
            //DataTable dataTable = _products.Select(p => new { ID = p.Id, Descripcion = p.Description }).ToDataTable(p => p);

            Assert.IsTrue(dataTable.Columns.Contains("ID"));
            Assert.IsTrue(dataTable.Columns.Contains("Description"));
        }

        [TestMethod]
        public void ToDataTable_PassAMemberAccessAsProjector_ShouldGenerateADataTableWithMemberAccessedAsColumn()
        {
            //Expression<Func<Product, object>> projector = p => new { ID = p.Id, p.Description };
            //Expression<Func<Product, object>> projector2 = p =>  p.Id;
            //Expression<Func<Product, object>> projector = p => p.Id;

            DataTable dataTable = _products.ToDataTable(p => p.Id);
            //DataTable dataTable = _products.Select(p => new { ID = p.Id, Descripcion = p.Description }).ToDataTable(p => p);

            Assert.IsTrue(dataTable.Columns.Contains("Id"));
        }

        [TestMethod]
        public void ToDataTable_PassAParameterAsProjector_ShouldGenerateADataTableWithThePropertiesListedByTheStaticTypeOfTheParameter()
        {
            //Expression<Func<Product, object>> projector = p => new { ID = p.Id, p.Description };
            //Expression<Func<Product, object>> projector2 = p =>  p.Id;
            //Expression<Func<Product, object>> projector = p => p.Id;

            DataTable dataTable = _products.ToDataTable(p => p);
            //DataTable dataTable = _products.Select(p => new { ID = p.Id, Descripcion = p.Description }).ToDataTable(p => p);

            Assert.IsTrue(dataTable.Columns.Contains("Id"));
        }
    }

    internal static class DataTableHelper
    {

    }

    public class Product
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}
