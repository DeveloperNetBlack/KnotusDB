using Microsoft.AspNetCore.Identity;

namespace Knotus.NET10.DB.SQLServer
{
    /// <summary>
    /// Marker class requerida por PasswordHasher&lt;T&gt;. No necesita contenido:
    /// solo sirve para tipar el hasher. Puedes reemplazarla por tu propia clase
    /// de entidad Usuario si prefieres, PasswordHasher no usa sus propiedades.
    /// </summary>
    public class UsuarioMarker { }

    /// <summary>
    /// Hash y verificación de contraseñas de usuario. A diferencia de Security.cs,
    /// esto es UNIDIRECCIONAL: no existe un método para "recuperar" la contraseña
    /// original a partir del hash — eso es intencional y es lo correcto para login.
    ///
    /// Usa PasswordHasher de ASP.NET Core Identity (PBKDF2 con salt aleatorio e
    /// iteraciones altas por defecto, actualizado por Microsoft con cada versión
    /// según recomendaciones vigentes de seguridad).
    /// </summary>
    public static class PasswordSecurity
    {
        private static readonly PasswordHasher<UsuarioMarker> _hasher = new();

        /// <summary>
        /// Genera el hash a guardar en la base de datos. Este es el valor que
        /// reemplaza a "la contraseña cifrada" en tu esquema actual.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "La contraseña no puede ser nula o vacía.");
            }

            return _hasher.HashPassword(new UsuarioMarker(), password);
        }

        /// <summary>
        /// Verifica una contraseña ingresada contra el hash guardado.
        /// needsRehash indica si conviene regenerar el hash (ej: si Microsoft
        /// subió las iteraciones recomendadas desde que se generó este hash) —
        /// si es true, tras un login exitoso conviene llamar HashPassword de nuevo
        /// y actualizar el valor guardado.
        /// </summary>
        public static bool VerifyPassword(string hashedPassword, string providedPassword, out bool needsRehash)
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }

            if (string.IsNullOrEmpty(providedPassword))
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            var resultado = _hasher.VerifyHashedPassword(new UsuarioMarker(), hashedPassword, providedPassword);

            needsRehash = resultado == PasswordVerificationResult.SuccessRehashNeeded;
            return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}