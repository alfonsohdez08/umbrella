using SampleApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Umbrella;

namespace SampleApp.Data
{
    class OrderRepository: BaseRepository
    {
        private const string GetOrdersWithSpecificProductsSp = "dbo.GetOrdersWithSpecificProducts";

        public OrderRepository(string connectionString): base(connectionString)
        {

        }


        public List<Order> GetOrdersWithSpecificProducts(List<Product> products)
        {
            List<Order> orders = new List<Order>();

            try
            {
                SqlConn.Open();

                using (SqlCommand sqlCommand = SqlConn.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = GetOrdersWithSpecificProductsSp;

                    sqlCommand.Parameters.AddWithValue("@Ids", products.ToDataTable(p => p.Id));

                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    while (dataReader.Read())
                    {
                        var order = new Order()
                        {
                            Id = dataReader.GetInt32(0),
                            PlacedDate = dataReader.GetDateTime(1)
                        };

                        orders.Add(order);
                    }
                }

            }
            finally
            {
                SqlConn.Close();
            }


            return orders;
        }
    }
}
