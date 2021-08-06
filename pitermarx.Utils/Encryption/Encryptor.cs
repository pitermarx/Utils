using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace pitermarx.Encryption
{
    // .Net Symetric encryption
    public class Encryptor<T>
        where T : SymmetricAlgorithm, new()
    {
        private const int KeySize = 256;
        private readonly byte[] vectorBytes;
        private readonly byte[] keyBytes;

        public Encryptor(
            string password,
            string salt = "my random salt 1",   // Random - Salt
            string vector = "1234567890123456") // Random - 16 lenght
        {
            vectorBytes = Encoding.ASCII.GetBytes(vector);
            var passwordBytes = new Rfc2898DeriveBytes(
                password,
                Encoding.ASCII.GetBytes(salt),
                hashAlgorithm: HashAlgorithmName.SHA1,
                iterations: 2);
            keyBytes = passwordBytes.GetBytes(KeySize / 8);
        }

        public string EncryptString(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);

            byte[] encrypted;
            using (var cipher = new T())
            {
                cipher.Mode = CipherMode.CBC;

                using (var encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                using (var to = new MemoryStream())
                using (var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                {
                    writer.Write(valueBytes, 0, valueBytes.Length);
                    writer.FlushFinalBlock();
                    encrypted = to.ToArray();
                }

                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        public string DecryptString(string value)
        {
            byte[] valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            int decryptedByteCount = 0;

            using (var cipher = new T())
            {
                cipher.Mode = CipherMode.CBC;

                using (var decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                using (var from = new MemoryStream(valueBytes))
                using (var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                {
                    decrypted = new byte[valueBytes.Length];
                    decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}