using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
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
        public void ToDataTable_PassAMemberInitProjection_ShouldGenerateDataTableBasedOnMembersInitialized()
        {
            Expression<Func<Product, Product>> projector = p => new Product() { Id = p.Id, Description = p.Description };

            DataTable dataTable = _products.ToDataTable(projector);

            Assert.IsTrue(dataTable.Columns.Contains("Id"));
            Assert.IsTrue(dataTable.Columns.Contains("Description"));
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