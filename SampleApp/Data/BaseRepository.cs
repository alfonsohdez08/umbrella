using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SampleApp.Data
{
    abstract class BaseRepository
    {
        protected SqlConnection SqlConn { get; private set; }

        public BaseRepository(string connectionString)
        {
            SqlConn = new SqlConnection(connectionString);
        }
    }
}
