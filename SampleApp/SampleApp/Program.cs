using SampleApp.Data;
using SampleApp.Data.Entities;
using System;
using System.Collections.Generic;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string connString = "Server=localhost\\SQLEXPRESS;Database=Northwind;Trusted_Connection=True;";

            var northwindCtx = new NorthwindContext(connString);

            var productService = new ProductService(northwindCtx);
            var orderService = new OrderService(northwindCtx, productService);

            List<Order> ordersWithExpensiveProducts = orderService.GetOrdersWithTopExpensiveProducts(3);

            List<Product> newProducts = GetProducts();
            productService.BulkInsertProducts(newProducts);

            Console.ReadLine();
        }

        static List<Product> GetProducts()
        {
            return new List<Product>()
            {
                new Product(){Name = "Nesquik", Price = 2m, MeasureUnit = "1 scoop", Discontinued = false},
                new Product(){Name = "Rum", Price = 11m, MeasureUnit = "1 lt", Discontinued = false},
                new Product(){Name = "Vodka", Price = 33m, MeasureUnit = "1 lt", Discontinued = false},
                new Product(){Name = "Rum box", Price = 8m, MeasureUnit = "box", Discontinued = true}
            };
        }
    }
}
