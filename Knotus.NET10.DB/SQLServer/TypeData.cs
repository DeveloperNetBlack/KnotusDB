using System.Data;

namespace Knotus.NET10.DB.SQLServer
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
