using System;
using System.Security.Cryptography;
using System.Text;

namespace Alkami.TrackableObjects.Plugins.Cryptography
{
    /// <summary>
    /// SymmetricAlgorithmFactory
    /// </summary>
    public static class SymmetricAlgorithmFactory
    {
	    /// <summary>
        /// Creates the specified spec.
        /// </summary>
        /// <returns></returns>
        public static SymmetricAlgorithm Create(BlockCipherSpec spec, string key,byte[] initializationVector=null)
        {
            var retVal = CreateWithoutKey(spec,initializationVector);
            retVal.Key = DeriveKey(spec.KeyDerivationFunction, key, retVal.KeySize);
            return retVal;
        }

        /// <summary>
        /// Creates the specified spec.
        /// </summary>
        /// <returns></returns>
        public static SymmetricAlgorithm Create(BlockCipherSpec spec, byte[] key,byte[] initializationVector=null)
        {
            if(spec.KeyDerivationFunction!=KeyDerivationFunction.Guess&&spec.KeyDerivationFunction!=KeyDerivationFunction.RawBytes)
                throw new InvalidOperationException("Chosen KeyDerivationFunction requires a string");
            var retVal = CreateWithoutKey(spec,initializationVector);
            //rawutf8 is lax about length. RawBytes is strict
            if (key == null || key.Length != retVal.KeySize/8)
            {
                var msg = (spec.Algorithm == BlockCipherAgorithm.TripleDES)
                    ? "Invalid key length (TripleDES expects keys in padded form)"
                    : "Invalid key length";
                throw new InvalidOperationException(msg);
            }
            retVal.Key = key;
            return retVal;
        }


        private static SymmetricAlgorithm CreateWithoutKey(BlockCipherSpec spec,byte[] initializationVector)
        {
            SymmetricAlgorithm retVal;
            switch (spec.Algorithm)
            {
                case BlockCipherAgorithm.AES:
		            retVal = new AesManaged
		            {
			            KeySize = spec.KeySize
		            };
		            break;
                case BlockCipherAgorithm.TripleDES:
                    retVal = new TripleDESCryptoServiceProvider();
                    //TripleDES class expects keys in padded form
                    if (spec.KeySize == 112)
                        retVal.KeySize = 128;
                    else if (spec.KeySize == 168)
                        retVal.KeySize = 192;
                    else
                        retVal.KeySize = spec.KeySize;
                    break;
                default:
                    throw new NotSupportedException(spec.Algorithm.ToString());
            }
            retVal.Mode = spec.Mode;
            retVal.Padding = spec.PaddingMode;
            if (spec.InitializationVectorGenerationMode != InitializationVectorGenerationMode.None)
                retVal.IV = GenerateInitializationVector(spec.InitializationVectorGenerationMode,
                    retVal.BlockSize);
            else if (initializationVector != null)
                retVal.IV = initializationVector;
            return retVal;
            
        }


        private static byte[] DeriveKey(KeyDerivationFunction function, string key, int nBits)
        {
            switch (function)
            {
                case KeyDerivationFunction.RawBytes:
                    throw new InvalidOperationException("KeyDerivationFunction.RawBytes selected but string key supplied");
                case KeyDerivationFunction.RawUtf8String:
                    var tmp = Encoding.UTF8.GetBytes(key);
                    if (tmp.Length == nBits / 8)
                        return tmp;
                    var retVal = new byte[nBits / 8];
                    Array.Copy(tmp,retVal,Math.Min(tmp.Length,retVal.Length));
                    return retVal;
                case KeyDerivationFunction.Base64String:
                    var tmp2 = Convert.FromBase64String(key);
                    if(tmp2.Length!=nBits/8)
                        throw new InvalidOperationException(String.Format("String decodes to {0} bits but key length should be {1}",tmp2.Length*8,nBits));
                    return tmp2;
                case KeyDerivationFunction.Guess:
                    try
                    {
                        return DeriveKey(KeyDerivationFunction.Base64String, key, nBits);
                    }
                    catch (Exception)
                    {
                        return DeriveKey(KeyDerivationFunction.RawUtf8String, key, nBits);
                    }
                default:
                    throw new NotSupportedException(function.ToString());
            }
        }

        /// <summary>
        /// Generates the initialization vector.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="nBits">The n bits.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">
        /// can only generate whole bytes
        /// or
        /// </exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static byte[] GenerateInitializationVector(InitializationVectorGenerationMode mode, int nBits)
        {
            if (nBits % 8 != 0)
                throw new NotSupportedException("can only generate whole bytes");
            switch (mode)
            {
                case InitializationVectorGenerationMode.Ascii:
		            var str = SymmetricAlgorithmUtility.GetRandomString(nBits / 8);
                    return Encoding.ASCII.GetBytes(str);
                case InitializationVectorGenerationMode.Binary:
                    return SymmetricAlgorithmUtility.GetRandomBits(nBits);
                case InitializationVectorGenerationMode.None:
                    throw new InvalidOperationException();
                default:
                    throw new NotSupportedException(mode.ToString());
            }
        }
    }
}
