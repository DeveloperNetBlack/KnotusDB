using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;


namespace CompileIT.DB.MySQL
{
    public class Segurity
    {
        private static readonly byte[] bytVectorIV = { 240, 3, 45, 29, 0, 76, 173, 59 };

        public static string CrytpoEncrypted(string claveUsuario, string idUsuario)
        {

            byte[] bytLlave;
            byte[] bytVector = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byte[] bytArreglo = Encoding.UTF8.GetBytes(claveUsuario);
            DESCryptoServiceProvider descCrypto = new DESCryptoServiceProvider();
            MemoryStream msCrypto = new MemoryStream();
            CryptoStream csCrypto;
            string strRetorno;

            idUsuario = idUsuario.ToUpper().PadLeft(8, '0');

            try
            {

                bytLlave = Encoding.UTF8.GetBytes(idUsuario.Substring(0, 8));
                csCrypto = new CryptoStream(msCrypto, descCrypto.CreateEncryptor(bytLlave, bytVector), CryptoStreamMode.Write);
                csCrypto.Write(bytArreglo, 0, bytArreglo.Length);
                csCrypto.FlushFinalBlock();

                strRetorno = Convert.ToBase64String(msCrypto.ToArray());

            }
            catch
            {
                strRetorno = "";
            }

            return strRetorno;

        }

        public static string CryptoDecrypted(string claveUsuario, string idUsuario)
        {

            byte[] bytLlave;
            byte[] bytVector = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byte[] bytArreglo;
            DESCryptoServiceProvider descCrypto = new DESCryptoServiceProvider();
            MemoryStream msCrypto = new MemoryStream();
            CryptoStream csCrypto;
            string strRetorno;

            idUsuario = idUsuario.ToUpper().PadLeft(8, '0');

            try
            {

                bytLlave = Encoding.UTF8.GetBytes(idUsuario.Substring(0, 8));
                bytArreglo = Convert.FromBase64String(claveUsuario);

                csCrypto = new CryptoStream(msCrypto, descCrypto.CreateDecryptor(bytLlave, bytVector), CryptoStreamMode.Write);
                csCrypto.Write(bytArreglo, 0, bytArreglo.Length);
                csCrypto.FlushFinalBlock();

                strRetorno = Encoding.UTF8.GetString(msCrypto.ToArray());

            }
            catch
            {

                strRetorno = "";
            }


            return strRetorno;

        }
    }
}
