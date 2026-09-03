using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Knotus.NET10.DB.SQLServer
{
    /// <summary>
    /// Clase que contiene los metodos necesarios para la ejecución de procedimientos en una base de datos
    /// Esta clase recibe como parametro de entrada la Entidad que se desea procesar. Esta clase sirve para la ejecución de devolución de registros.
    /// </summary>
    /// <typeparam name="T">Entidad que recibirá la información cuando se ejecuta un procedimiento en la base de datos</typeparam>
    public class Connection<T> where T : new()
    {
        #region "Definición de variables locales de la clase"

        private TypeRefund.Register tipoDevolucion;
        private object? objEscalar;
        private IList<T> lstEntidad = new List<T>();
        private T? entidadUnica;
        private DataSet dsDato = new DataSet();

        // Cache de PropertyInfo por tipo, para no usar reflection en cada fila.
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

        #endregion

        #region "Constructor de la clase"

        private readonly IConfiguration _configuration;

        public Connection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region "Propiedades"

        public TypeRefund.Register Devolution
        {
            get => tipoDevolucion;
            set => tipoDevolucion = value;
        }

        public DataSet ReturnDataset => dsDato;

        public object? ReturnScale => objEscalar;

        public IList<T> ReturnEntity => lstEntidad;

        public T? ReturnEntitySingle => entidadUnica;

        private string StringConexion
        {
            get
            {
                string? valorConnection = _configuration.GetSection("ConnectionStrings")["ConnectionSQLServer"];
                return Security.Decrypt(valorConnection, _configuration["Security:ConnectionStringPassphrase"]);
            }
        }

        #endregion

        #region "Metodos para la ejecución en la base de datos"

        /// <summary>
        /// Limpia el estado de ejecuciones anteriores. Se llama al inicio de cada Execute*
        /// para que la instancia se pueda reutilizar sin arrastrar datos previos.
        /// </summary>
        private void ResetState()
        {
            dsDato = new DataSet();
            lstEntidad = new List<T>();
            entidadUnica = default;
            objEscalar = null;
        }

        private static SqlCommand BuildCommand(SqlConnection conn, Parameters objParametros)
        {
            var cmdComando = new SqlCommand
            {
                CommandTimeout = 120, // segundos. Ajustar según el SP más lento conocido.
                CommandText = objParametros.NameProcedure,
                CommandType = CommandType.StoredProcedure,
                Connection = conn
            };

            foreach (var p in objParametros.ListParameters)
            {
                var prmParametro = new SqlParameter
                {
                    ParameterName = p.NameParameter,
                    Direction = p.Direction,
                    SqlDbType = (SqlDbType)p.TypeData,
                    Size = p.Large,
                    Value = p.Value ?? DBNull.Value
                };

                cmdComando.Parameters.Add(prmParametro);
            }

            return cmdComando;
        }

        /// <summary>
        /// Permite ejecutar procedimientos almacenados que retornan conjuntos de registros
        /// </summary>
        /// <param name="objParametros">Parametros del procedimiento</param>
        public void ExecuteSQL(Parameters objParametros)
        {
            ResetState();

            using var objConn = new SqlConnection(StringConexion);
            objConn.Open();

            using var cmdComando = BuildCommand(objConn, objParametros);
            using var sdaEjecuta = new SqlDataAdapter(cmdComando);

            sdaEjecuta.Fill(dsDato);

            switch (tipoDevolucion)
            {
                case TypeRefund.Register.Entity:
                    if (dsDato.Tables.Count > 0)
                    {
                        lstEntidad = ConvertToList(dsDato.Tables[0]);
                    }
                    break;

                case TypeRefund.Register.EntitySingle:
                    if (dsDato.Tables.Count > 0)
                    {
                        var item = ConvertToList(dsDato.Tables[0]);
                        if (item.Count > 0)
                        {
                            entidadUnica = item[0];
                        }
                    }
                    break;

                case TypeRefund.Register.Scale:
                    if (dsDato.Tables.Count > 0 && dsDato.Tables[0].Rows.Count > 0)
                    {
                        objEscalar = dsDato.Tables[0].Rows[0][0];
                    }
                    break;
            }
        }

        /// <summary>
        /// Permite ejecutar procedimientos almacenados que retornan un valor escalar
        /// </summary>
        /// <param name="objParametros">Parámetros del procedimiento</param>
        public void ExecuteScale(Parameters objParametros)
        {
            ResetState();

            using var objConn = new SqlConnection(StringConexion);
            objConn.Open();

            using var cmdComando = BuildCommand(objConn, objParametros);
            using var sdaEjecuta = new SqlDataAdapter(cmdComando);

            sdaEjecuta.Fill(dsDato);

            if (dsDato.Tables.Count > 0 && dsDato.Tables[0].Rows.Count > 0)
            {
                objEscalar = dsDato.Tables[0].Rows[0][0];
            }
        }

        /// <summary>
        /// Versión asíncrona de ExecuteSQL. Usa SqlDataReader en vez de SqlDataAdapter
        /// porque SqlDataAdapter.Fill no tiene una variante async real.
        /// </summary>
        public async Task ExecuteSQLAsync(Parameters objParametros, CancellationToken ct = default)
        {
            ResetState();

            await using var objConn = new SqlConnection(StringConexion);
            await objConn.OpenAsync(ct);

            await using var cmdComando = BuildCommand(objConn, objParametros);

            using var dtTabla = new DataTable();
            await using var reader = await cmdComando.ExecuteReaderAsync(ct);
            dtTabla.Load(reader);
            dsDato.Tables.Add(dtTabla);

            switch (tipoDevolucion)
            {
                case TypeRefund.Register.Entity:
                    lstEntidad = ConvertToList(dtTabla);
                    break;

                case TypeRefund.Register.EntitySingle:
                    var item = ConvertToList(dtTabla);
                    if (item.Count > 0)
                    {
                        entidadUnica = item[0];
                    }
                    break;

                case TypeRefund.Register.Scale:
                    if (dtTabla.Rows.Count > 0)
                    {
                        objEscalar = dtTabla.Rows[0][0];
                    }
                    break;
            }
        }

        #endregion

        #region "Metodos de Conversión de a Lista"

        public static IList<T> ConvertToList(DataTable dtTabla)
        {
            IList<T> lstLista = new List<T>();

            if (dtTabla == null || dtTabla.Rows.Count == 0)
            {
                return lstLista;
            }

            var mapaPropiedades = GetPropertyMap(dtTabla);

            foreach (DataRow drFila in dtTabla.Rows)
            {
                T objEntidad = ConvertDataRowEntidad(drFila, mapaPropiedades);
                lstLista.Add(objEntidad);
            }

            return lstLista;
        }

        /// <summary>
        /// Construye (o recupera de cache) el mapa columna->propiedad para el tipo T,
        /// evitando reflection repetida por cada fila.
        /// </summary>
        private static Dictionary<string, PropertyInfo> GetPropertyMap(DataTable dtTabla)
        {
            Type objTipo = typeof(T);

            if (_propertyCache.TryGetValue(objTipo, out var cached))
            {
                return cached;
            }

            var propiedades = objTipo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mapa = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (DataColumn dcColumna in dtTabla.Columns)
            {
                string nombreNormalizado = dcColumna.ColumnName.Replace("_", "");

                var propiedad = propiedades.FirstOrDefault(p =>
                    string.Equals(p.Name, nombreNormalizado, StringComparison.OrdinalIgnoreCase));

                if (propiedad != null && propiedad.CanWrite)
                {
                    mapa[dcColumna.ColumnName] = propiedad;
                }
            }

            _propertyCache[objTipo] = mapa;
            return mapa;
        }

        private static T ConvertDataRowEntidad(DataRow drFila, Dictionary<string, PropertyInfo> mapaPropiedades)
        {
            object objInstancia = new T()!;

            foreach (DataColumn dcColumna in drFila.Table.Columns)
            {
                if (!mapaPropiedades.TryGetValue(dcColumna.ColumnName, out var pPropiedad))
                {
                    continue;
                }

                object? objValor = drFila[dcColumna.ColumnName];

                if (objValor == DBNull.Value)
                {
                    objValor = null;
                }
                else if (objValor is string strValor
                         && pPropiedad.PropertyType != typeof(string)
                         && (strValor.TrimStart().StartsWith('[') || strValor.TrimStart().StartsWith('{')))
                {
                    // La columna viene como JSON (ej. FOR JSON PATH) y la propiedad destino es un tipo complejo (List<T>, objeto, etc.)
                    objValor = JsonSerializer.Deserialize(strValor, pPropiedad.PropertyType, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                pPropiedad.SetValue(objInstancia, objValor, null);
            }

            return (T)objInstancia;
        }

        #endregion
    }
}