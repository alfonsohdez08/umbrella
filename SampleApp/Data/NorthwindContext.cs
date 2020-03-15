using System;
using System.Collections.Generic;
using System.Text;

namespace SampleApp.Data
{
    class NorthwindContext
    {
        public OrderRepository Orders { get; private set; }
        public ProductRepository Products { get; private set; }

        public NorthwindContext(string connectionString)
        {
            Orders = new OrderRepository(connectionString);
            Products = new ProductRepository(connectionString);
        }
    }
}
