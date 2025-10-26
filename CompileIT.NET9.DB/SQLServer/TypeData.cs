using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileIT.NET9.DB.SQLServer
{
    public class TypeData
    {
        public enum DataType
        {
            Varchar = SqlDbType.VarChar,
            Int = SqlDbType.Int,
            DateTime = SqlDbType.DateTime,
            Date = SqlDbType.Date,
            Time = SqlDbType.Time,
            Boolean = SqlDbType.Bit,
            Decimal = SqlDbType.Decimal,
            Float = SqlDbType.Float,
            Text = SqlDbType.Text
        }
    }
}
