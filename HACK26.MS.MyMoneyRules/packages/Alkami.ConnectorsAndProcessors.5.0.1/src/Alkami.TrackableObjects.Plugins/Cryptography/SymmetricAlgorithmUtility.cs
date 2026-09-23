using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Alkami.TrackableObjects.Plugins.Cryptography
{
    /// <summary>
    /// SymmetricAlgorithmUtility
    /// </summary>
    public static class SymmetricAlgorithmUtility
    {
        /// <summary>
        /// Encrypts the specified algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="plaintext">The plaintext.</param>
        /// <returns></returns>
        public static byte[] Encrypt(this SymmetricAlgorithm algorithm, string plaintext)
        {
            var enc = algorithm.CreateEncryptor();
            var pBytes = Encoding.UTF8.GetBytes(plaintext);
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, enc, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(pBytes, 0, pBytes.Length);
                }
                return msEncrypt.ToArray();
            }
        }

	    private static readonly RNGCryptoServiceProvider CryptoRandom = new RNGCryptoServiceProvider();

	    internal static byte[] GetRandomBits(int nBits)
	    {
		    var bytes = new byte[nBits / 8];
		    CryptoRandom.GetBytes(bytes);
		    return bytes;

	    }

	    /// <summary>
	    /// Gets the random string.
	    /// </summary>
	    /// <param name="length">The length.</param>
	    /// <returns></returns>
	    public static string GetRandomString(int length)
	    {
		    var nBits = length * 8;
		    var nEntropyBits = (nBits * 3 + 3) / 4;// base64 is 3/4 size, round up to whole byte
		    var bytes = GetRandomBits(nEntropyBits);
		    var str = Convert.ToBase64String(bytes);

		    if (str.Length > length)
		    {
				str = str.Substring(0, length);
			}
			return str;
	    }

	    /// <summary>
        /// Encrypts to base64.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="plaintext">The plaintext.</param>
        /// <returns></returns>
        public static string EncryptToBase64(this SymmetricAlgorithm algorithm, string plaintext)
        {
            var tmp = algorithm.Encrypt(plaintext);
            return Convert.ToBase64String(tmp);
        }

        /// <summary>
        /// Decrypts the specified algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="base64Ciphertext">The base64 ciphertext.</param>
        /// <returns></returns>
        public static string Decrypt(this SymmetricAlgorithm algorithm, string base64Ciphertext)
        {
            var bytes = Convert.FromBase64String(base64Ciphertext);
            return algorithm.Decrypt(bytes);
        }

        /// <summary>
        /// Decrypts the specified algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string Decrypt(this SymmetricAlgorithm algorithm, byte[] bytes)
        {
            var decryptor = algorithm.CreateDecryptor();

            using (var msDecrypt = new MemoryStream(bytes))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
