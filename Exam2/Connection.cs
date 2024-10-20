using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam2
{
    internal class Connection
    {
        SqlConnection conn;
        public SqlConnection getCon()
        {
            conn = new SqlConnection("Data Source=LAPTOP-6L6RBUV1\\SQLEXPRESS;Initial Catalog=FarmDB;Integrated Security=True");
            return conn;
        }
    }
}
