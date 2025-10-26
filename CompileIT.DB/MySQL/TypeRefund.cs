using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CompileIT.DB.MySQL
{
    public class TypeRefund
    {
        public enum Register
        {
            Entity = 1,
            EntitySingle = 2,
            Dataset = 3,
            Scale = 4,
            None = 5
        }

    }
}
