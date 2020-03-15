using SampleApp.Data;
using SampleApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleApp
{
    class OrderService
    {
        private readonly NorthwindContext _northwindCtx;
        private readonly ProductService _productService;

        public OrderService(NorthwindContext northwindContext, ProductService productService)
        {
            _northwindCtx = northwindContext;
            _productService = productService;
        }

        public List<Order> GetOrdersWithTopExpensiveProducts(int productsCount = 5)
        {
            List<Product> expensiveProducts = _productService.GetTopExpensiveProducts(productsCount);
            List<Order> orders = _northwindCtx.Orders.GetOrdersWithSpecificProducts(expensiveProducts);

            return orders;
        }
    }
}
