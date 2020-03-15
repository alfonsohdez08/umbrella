using SampleApp.Data;
using SampleApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleApp
{
    class ProductService
    {
        private readonly NorthwindContext _northwindCtx;

        public ProductService(NorthwindContext northwindContext)
        {
            _northwindCtx = northwindContext;
        }

        public List<Product> GetTopExpensiveProducts(int productsCount = 5) => _northwindCtx.Products.GetTopExpensiveProducts(productsCount);

            
        public void BulkInsertProducts(List<Product> products) => _northwindCtx.Products.AddProducts(products);
    }
}
