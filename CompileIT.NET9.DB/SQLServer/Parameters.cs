using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileIT.NET9.DB.SQLServer
{
    public class Parameters
    {
        #region "Definición de variables locales"

        private readonly List<Parameters> listaParametros = new List<Parameters>();

        #endregion

        #region "Propiedades"

        public string? NameParameter { get; set; }
        public TypeData.DataType TypeData { get; set; }
        public int Large { get; set; }
        public ParameterDirection Direction { get; set; }
        public object? Value { get; set; }
        public string? NameProcedure { get; set; }

        public List<Parameters> ListParameters
        {
            get { return listaParametros; }
        }
        #endregion

        #region "Metodos"

        public void addParameters(string name, TypeData.DataType data, int large, ParameterDirection parameterDirection, object value)
        {
            Parameters parametro = new Parameters();

            parametro.NameParameter = name;
            parametro.TypeData = data;
            parametro.Large = large;
            parametro.Direction = parameterDirection;
            parametro.Value = value;

            listaParametros.Add(parametro);

        }

        #endregion
    }
}
