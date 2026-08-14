using System.Data;

namespace Knotus.NET10.DB.SQLServer
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

        /// <summary>
        /// Expuesta como solo lectura: la única forma de agregar parámetros
        /// es a través de AddParameter, para evitar que se manipule la lista
        /// directamente (Clear, Add, etc.) desde fuera de la clase.
        /// </summary>
        public IReadOnlyList<Parameters> ListParameters => listaParametros;

        #endregion

        #region "Metodos"

        /// <summary>
        /// Agrega un parámetro a la colección. Para parámetros de salida
        /// (ParameterDirection.Output) puedes omitir value.
        /// </summary>
        public void AddParameter(string name, TypeData.DataType data, int large, ParameterDirection parameterDirection, object? value = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre del parámetro no puede estar vacío.", nameof(name));
            }

            if (listaParametros.Any(p => string.Equals(p.NameParameter, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Ya existe un parámetro llamado '{name}' en esta colección.");
            }

            var parametro = new Parameters
            {
                NameParameter = name,
                TypeData = data,
                Large = large,
                Direction = parameterDirection,
                Value = value
            };

            listaParametros.Add(parametro);
        }

        /// <summary>
        /// Alias del método anterior, mantenido por compatibilidad con código existente
        /// que ya llama a addParameters. Marcar como Obsolete ayuda a migrar los llamadores
        /// gradualmente hacia AddParameter sin romper la compilación.
        /// </summary>
        [Obsolete("Usar AddParameter en su lugar. Este método se mantiene solo por compatibilidad.")]
        public void addParameters(string name, TypeData.DataType data, int large, ParameterDirection parameterDirection, object? value = null)
        {
            AddParameter(name, data, large, parameterDirection, value);
        }

        #endregion
    }
}