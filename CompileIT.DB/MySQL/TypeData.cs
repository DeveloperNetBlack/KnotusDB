using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace CompileIT.DB.MySQL
{
    public class TypeData
    {
        public enum DataType
        {
            Varchar = MySqlDbType.VarChar,
            Int64 = MySqlDbType.Int64,
            Int32 = MySqlDbType.Int32,
            Double = MySqlDbType.Double,
            DateTime = MySqlDbType.DateTime,
            Date = MySqlDbType.Date,
            Time = MySqlDbType.Time,
            Boolean = MySqlDbType.Bit,
            Decimal = MySqlDbType.Decimal,
            Float = MySqlDbType.Float,
            Text = MySqlDbType.Text
        }
    }
}
