using System.Security.Cryptography;
using System.Text;

namespace CompileIT.NET9.DB.SQLServer
{
    public static class Security
    {
        private static byte[] IV = Encoding.UTF8.GetBytes("q3&9z$q31*_t6?z$");

        public static string Encrypt(string? plainText, string? keyTexto)
        {
            using Aes aes = Aes.Create();

            if (string.IsNullOrEmpty(keyTexto))
            {
                throw new ArgumentNullException(nameof(keyTexto), "La clave no puede ser nula o vacía.");
            }

            if (keyTexto.Length != 32)
            {
                keyTexto = keyTexto.PadRight(32, '*');
            }

            byte[] Llave = Encoding.UTF8.GetBytes(keyTexto);

            aes.Key = Llave;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string Decrypt(string? cipheredText, string? keyTexto)
        {
            using Aes aes = Aes.Create();

            if (string.IsNullOrEmpty(keyTexto))
            {
                throw new ArgumentNullException(nameof(keyTexto), "La clave no puede ser nula o vacía.");
            }

            if (keyTexto.Length != 32)
            {
                keyTexto = keyTexto.PadRight(32, '*');
            }

            byte[] Key = Encoding.UTF8.GetBytes(keyTexto);

            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            if (string.IsNullOrEmpty(cipheredText))
            {
                throw new ArgumentNullException(nameof(cipheredText), "La cipheredText no puede ser nula o vacía.");
            }

            byte[] cipheredBytes = Convert.FromBase64String(cipheredText);
            using MemoryStream msEncrypt = new(cipheredBytes);
            using CryptoStream csEncrypt = new(msEncrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csEncrypt);

            return srDecrypt.ReadToEnd();
        }
    }
}
