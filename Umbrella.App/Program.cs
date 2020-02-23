using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbrella.App
{

    public class Order
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsShipped { get; set; }
        public decimal Amount { get; set; }
    }

    class Program
    {
        // im only have to produce flat results (i mean, there's not concept of hierarchy objects when creating a tvp)

        static void Main(string[] args)
        {
            var orders = new List<Order>()
            {
                new Order(){ Id = 1, Description = "ABC", IsShipped = false, Amount = 10},
                new Order(){ Id = 2, Description = "XYZ", IsShipped = true, Amount = 1021},
                new Order(){ Id = 3, Description = "AXA", IsShipped = false, Amount = 2210},
                new Order(){ Id = 4, Description = "SDA", IsShipped = true, Amount = 440}
            };

            var dataTable = orders.ToDataTable(o => new { o.Id, Desc = o.Description, o.Amount });
            //var dataTable = orders.ToDataTable(o => o); // this is failling


            Console.ReadLine();
        }
    }

}
