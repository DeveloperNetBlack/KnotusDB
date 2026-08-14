using System.Security.Cryptography;
using System.Text;

namespace Knotus.NET10.DB.SQLServer
{
    /// <summary>
    /// Cifrado/descifrado reversible para secretos que la aplicación necesita
    /// recuperar en texto plano (connection strings, API keys de terceros, etc).
    ///
    /// IMPORTANTE: esta clase NO debe usarse para contraseñas de login de usuarios.
    /// Para eso usar Microsoft.AspNetCore.Identity.PasswordHasher (ver PasswordSecurity.cs),
    /// que es unidireccional (hash) y no permite recuperar la contraseña original.
    ///
    /// Usa AES-256-GCM: modo autenticado, protege contra manipulación del texto
    /// cifrado (a diferencia de CBC solo). El IV se genera aleatoriamente en cada
    /// Encrypt y se guarda junto al resultado (no necesita ser secreto).
    /// </summary>
    public static class Security
    {
        private const int NonceSize = 12;  // 96 bits, tamaño recomendado para GCM
        private const int TagSize = 16;    // 128 bits, tamaño del tag de autenticación
        private const int KeySize = 32;    // AES-256
        private const int SaltSize = 16;
        private const int Pbkdf2Iterations = 210_000; // recomendación OWASP 2024+ para PBKDF2-SHA256

        /// <summary>
        /// Deriva una clave AES-256 real a partir de una passphrase usando PBKDF2,
        /// en vez de rellenar el texto con caracteres fijos (lo que hacía la versión anterior).
        /// </summary>
        private static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(passphrase),
                salt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
        }

        /// <summary>
        /// Cifra un texto. El resultado incluye salt + nonce + tag + ciphertext,
        /// todo en un solo Base64, para que Decrypt no necesite parámetros extra.
        /// </summary>
        public static string Encrypt(string? plainText, string? passphrase)
        {
            if (string.IsNullOrEmpty(passphrase))
            {
                throw new ArgumentNullException(nameof(passphrase), "La clave no puede ser nula o vacía.");
            }

            if (plainText == null)
            {
                throw new ArgumentNullException(nameof(plainText), "El texto a cifrar no puede ser nulo.");
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = DeriveKey(passphrase, salt);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            // Formato de salida: salt(16) + nonce(12) + tag(16) + ciphertext
            byte[] resultado = new byte[SaltSize + NonceSize + TagSize + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, resultado, 0, SaltSize);
            Buffer.BlockCopy(nonce, 0, resultado, SaltSize, NonceSize);
            Buffer.BlockCopy(tag, 0, resultado, SaltSize + NonceSize, TagSize);
            Buffer.BlockCopy(cipherBytes, 0, resultado, SaltSize + NonceSize + TagSize, cipherBytes.Length);

            return Convert.ToBase64String(resultado);
        }

        /// <summary>
        /// Descifra un texto generado por Encrypt. Lanza CryptographicException
        /// si el texto fue manipulado (falla la verificación del tag de GCM).
        /// </summary>
        public static string Decrypt(string? cipheredText, string? passphrase)
        {
            if (string.IsNullOrEmpty(passphrase))
            {
                throw new ArgumentNullException(nameof(passphrase), "La clave no puede ser nula o vacía.");
            }

            if (string.IsNullOrEmpty(cipheredText))
            {
                throw new ArgumentNullException(nameof(cipheredText), "El texto cifrado no puede ser nulo o vacío.");
            }

            byte[] datos = Convert.FromBase64String(cipheredText);

            if (datos.Length < SaltSize + NonceSize + TagSize)
            {
                throw new CryptographicException("El texto cifrado tiene un formato inválido.");
            }

            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[datos.Length - SaltSize - NonceSize - TagSize];

            Buffer.BlockCopy(datos, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(datos, SaltSize, nonce, 0, NonceSize);
            Buffer.BlockCopy(datos, SaltSize + NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(datos, SaltSize + NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

            byte[] key = DeriveKey(passphrase, salt);
            byte[] plainBytes = new byte[cipherBytes.Length];

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                // Si el texto fue manipulado o la clave es incorrecta, esto lanza
                // CryptographicException en vez de devolver datos corruptos silenciosamente.
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}