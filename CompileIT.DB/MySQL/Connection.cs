using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Reflection;

namespace CompileIT.DB.MySQL
{
    /// <summary>
    /// Clase que contiene los metodos necesarios para la ejecución de procedimientos en una base de datos
    /// Esta clase recibe como parametro de entrada la Entidad que se desea procesar. Esta clase sirve para la ejecución de devolución de registros.
    /// </summary>
    /// <typeparam name="T">Entidad que recibirá la información cuando se ejecuta un procedimiento en la base de datos</typeparam>
    public class Connection<T> : IEnumerable<T>, ICollection<T>, IList<T>
    {
        #region "Definición de variables locales de la clase"

        private MySqlConnection objConn;
        private TypeRefund.Register tipoDevolucion;
        private readonly ArrayList arrEntidad = new ArrayList();
        private object objEscalar;
        private readonly IList<T> lstEntidad = new List<T>();
        private T entidadUnica;
        private readonly IList<T> lista = new List<T>();
        private readonly DataSet dsDato = new DataSet();

        //private readonly string strUsuarioBD = ConfigurationManager.GetSection("USUARIO").ToString();
        //private readonly string strPassword = Segurity.CryptoDecrypted(ConfigurationManager.GetSection("CLAVE").ToString(),
        //                                                                    ConfigurationManager.GetSection("USUARIO").ToString());
        //private readonly string strServidor = ConfigurationManager.GetSection("SERVIDOR").ToString();
        //private readonly string strBaseDatos = ConfigurationManager.GetSection("BASEDATOS").ToString();
        //private string strConexionString;

        #endregion

        #region "Implementar IEnumerable"

        public IEnumerator<T> GetEnumerator()
        {
            return lista.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region "Implementar ICollection<T>"

        public void Add(T item)
        {
            lista.Add(item);
        }

        public void Clear()
        {
            lista.Clear();
        }

        public bool Contains(T item)
        {
            return lista.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lista.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return lista.Remove(item);
        }

        public int Count
        {
            get
            {
                return lista.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return lista.IsReadOnly;
            }
        }

        #endregion

        #region "Implementar IList<T>"

        public int IndexOf(T item)
        {
            return lista.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            lista.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lista.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return lista[index];
            }
            set
            {
                lista[index] = value;
            }
        }

        #endregion

        #region "Propiedades"

        public TypeRefund.Register Devolution
        {
            get
            {
                return tipoDevolucion;
            }
            set
            {
                tipoDevolucion = value;
            }
        }

        public DataSet ReturnDataset
        {
            get
            {
                return dsDato;
            }
        }

        public object ReturnScale
        {
            get
            {
                return objEscalar;
            }
        }

        public ArrayList ReturnEntityArray
        {
            get
            {
                return arrEntidad;
            }
        }

        public IList<T> ReturnEntity
        {
            get
            {
                return lstEntidad;
            }
        }

        public T ReturnEntitySingle
        {
            get
            {
                return entidadUnica;
            }
        }

        public string StringConexion { get; set; }

        #endregion

        #region "Metodos para la ejecución en la base de datos"

        private void CloseDB()
        {
            objConn.Close();
        }

        private void OpenDB()
        {
            objConn = new MySqlConnection();
            objConn.ConnectionString = StringConexion; ;
            objConn.Open();
        }

        public void ExecuteSQL(Parameters objParametros)
        {
            MySqlCommand cmdComando;
            MySqlParameter prmParametro;
            MySqlDataAdapter sdaEjecuta;
            IList<T> item;

            OpenDB(); // Permite abrir la conexión a la base de datos.

            cmdComando = new MySqlCommand();
            cmdComando.CommandTimeout = 360000;
            cmdComando.CommandText = objParametros.NameProcedure;
            cmdComando.CommandType = CommandType.StoredProcedure;
            cmdComando.Connection = objConn;

            for (int intFila = 0; intFila < objParametros.ListParameters.Count; intFila++)
            {
                prmParametro = new MySqlParameter();
                prmParametro.ParameterName = objParametros.ListParameters[intFila].NameParameter;
                prmParametro.Direction = objParametros.ListParameters[intFila].Direction;
                prmParametro.MySqlDbType = (MySqlDbType)objParametros.ListParameters[intFila].TypeData;
                prmParametro.Size = objParametros.ListParameters[intFila].Large;
                prmParametro.Value = objParametros.ListParameters[intFila].Value;

                cmdComando.Parameters.Add(prmParametro);
            }

            sdaEjecuta = new MySqlDataAdapter(cmdComando);
            sdaEjecuta.Fill(dsDato);

            if (tipoDevolucion == TypeRefund.Register.Entity)
            {
                item = ConvertToList(dsDato.Tables[0]);
                arrEntidad.Add(item);

                if (item != null)
                {
                    for (int intFila = 0; intFila < item.Count; intFila++)
                    {
                        lstEntidad.Add(item[intFila]);
                    }
                }
            }
            else if (tipoDevolucion == TypeRefund.Register.EntitySingle)
            {
                if (dsDato.Tables.Count > 0)
                {
                    item = ConvertToList(dsDato.Tables[0]);

                    if (item != null)
                    {
                        entidadUnica = item[0];
                    }
                }
            }

            CloseDB(); // Permite cerrar la conexión a la base de datos.

        }

        public void ExeuteScale(Parameters objParametros)
        {
            MySqlCommand cmdComando;
            MySqlParameter prmParametro;
            MySqlDataAdapter sdaEjecuta;

            OpenDB(); // Permite abrir la conexión a la base de datos.

            cmdComando = new MySqlCommand();
            cmdComando.CommandTimeout = 360000;
            cmdComando.CommandText = objParametros.NameProcedure;
            cmdComando.CommandType = CommandType.StoredProcedure;
            cmdComando.Connection = objConn;

            for (int intFila = 0; intFila < objParametros.ListParameters.Count; intFila++)
            {
                prmParametro = new MySqlParameter();
                prmParametro.ParameterName = objParametros.ListParameters[intFila].NameParameter;
                prmParametro.Direction = objParametros.ListParameters[intFila].Direction;
                prmParametro.MySqlDbType = (MySqlDbType)objParametros.ListParameters[intFila].TypeData;
                prmParametro.Size = objParametros.ListParameters[intFila].Large;
                prmParametro.Value = objParametros.ListParameters[intFila].Value;

                cmdComando.Parameters.Add(prmParametro);
            }

            sdaEjecuta = new MySqlDataAdapter(cmdComando);
            sdaEjecuta.Fill(dsDato);

            if (tipoDevolucion == TypeRefund.Register.EntitySingle)
            {
                objEscalar = dsDato.Tables[0].Rows[0][0];
            }

            CloseDB(); // Permite cerrar la conexión a la base de datos.

        }

        #endregion

        #region "Metodos de Conversión de a Lista"

        public static IList<T> ConvertToList(DataTable dtTabla)
        {
            if (dtTabla == null || dtTabla.Rows.Count == 0)
            {
                return null;
            }

            IList<T> lstLista = new List<T>();

            foreach (DataRow drFila in dtTabla.Rows)
            {
                T objEntidad = ConvertDataRowEntidad(drFila);
                lstLista.Add(objEntidad);
            }

            return lstLista;
        }

        public static T ConvertDataRowEntidad(DataRow drFila)
        {
            Type objTipo = typeof(T);
            T objInstancia = Activator.CreateInstance<T>();
            string strNombreCampo;
            foreach (DataColumn dcColumna in drFila.Table.Columns)
            {
                strNombreCampo = dcColumna.ColumnName;
                dcColumna.ColumnName = dcColumna.ColumnName.Replace("_", "");

                PropertyInfo pPropiedad = objTipo.GetProperty(dcColumna.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (pPropiedad == null || !pPropiedad.CanWrite)
                {
                    continue;
                }

                object objValor = drFila[dcColumna.ColumnName];

                if (objValor == DBNull.Value)
                    objValor = null;
                pPropiedad.SetValue(objInstancia, objValor, null);
                dcColumna.ColumnName = strNombreCampo;
            }

            return objInstancia;
        }

        #endregion
    }
}
