using SampleApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Umbrella;

namespace SampleApp.Data
{
    class ProductRepository: BaseRepository
    {
        private const string GetTopExpensiveProductsSql = "select ProductID, UnitPrice from dbo.Products order by UnitPrice desc offset 0 rows fetch first @Count rows only";
        private const string InsertProductsSp = "dbo.InsertProducts";

        public ProductRepository(string connectionString) : base(connectionString)
        {

        }


        public List<Product> GetTopExpensiveProducts(int count)
        {
            List<Product> products = new List<Product>();

            try
            {
                SqlConn.Open();

                using (SqlCommand sqlCommand = SqlConn.CreateCommand())
                {
                    sqlCommand.CommandText = GetTopExpensiveProductsSql;
                    sqlCommand.Parameters.AddWithValue("@Count", count);

                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    while (dataReader.Read())
                    {
                        var product = new Product()
                        {
                            Id = dataReader.GetInt32(0),
                            Price = dataReader.GetDecimal(1)
                        };

                        products.Add(product);
                    }
                }
            }
            finally
            {
                SqlConn.Close();
            }


            return products;
        }

        public void AddProducts(List<Product> products)
        {

            try
            {
                SqlConn.Open();

                using (SqlCommand sqlCommand = SqlConn.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = InsertProductsSp;

                    DataTable productsDataTable = products.ToDataTable(p => new
                    {
                        p.Name,
                        p.SupplierId,
                        p.CategoryId,
                        MeasureUnit = ColumnSettings.Build(() => p.MeasureUnit).Name("Quantity Per Unit"),
                        UnitPrice = p.Price * 1.5m,
                        p.UnitsInStock,
                        p.UnitsOnOrder,
                        p.ReorderLevel,
                        p.Discontinued
                    });

                    sqlCommand.Parameters.AddWithValue("@Products", productsDataTable);

                    sqlCommand.ExecuteNonQuery();
                }
            }
            finally
            {
                SqlConn.Close();
            }
        }
    }
}
