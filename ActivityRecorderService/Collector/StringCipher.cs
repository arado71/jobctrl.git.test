using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tct.ActivityRecorderService.Collector
{
    public class StringCipher : IDisposable
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        private const string passPhrase = "EK$Bz._qv7p9]EA$";
        private static byte[] saltStringBytes = { 114, 80, 152, 222, 84, 147, 101, 182, 41, 116, 137, 28, 205, 182, 193, 235, 168, 121, 10, 171, 86, 155, 87, 60, 228, 252, 74, 182, 123, 17, 42, 153 };
        private static byte[] ivStringBytes = { 191, 253, 8, 39, 58, 248, 231, 220, 209, 68, 9, 197, 114, 15, 107, 37, 176, 115, 255, 221, 15, 170, 197, 113, 220, 205, 224, 221, 225, 167, 132, 129 };

        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public StringCipher()
        {
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            var keyBytes = password.GetBytes(Keysize / 8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.BlockSize = 256;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;

            encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
        }

        public string Encrypt(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var plainTextBytes = new byte[cipherTextBytes.Length];
                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    encryptor.Dispose();
                    decryptor.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
