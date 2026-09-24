using System.Security.Cryptography;

namespace Alkami.TrackableObjects.Plugins.Cryptography
{
    /// <summary>
    /// BlockCipherSpec
    /// </summary>
    public class BlockCipherSpec
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCipherSpec"/> class.
        /// </summary>
        public BlockCipherSpec()
        {
            Algorithm=BlockCipherAgorithm.AES;
            KeySize = 256;//TODO: correctly handle 64 vs 56 for des
            Mode=CipherMode.CBC;
            PaddingMode = PaddingMode.PKCS7;
            KeyDerivationFunction = KeyDerivationFunction.Guess;
            InitializationVectorGenerationMode = InitializationVectorGenerationMode.Binary;

        }

        /// <summary>
        /// aes256 PKCS7 CBC
        /// </summary>
        public static BlockCipherSpec DefaultAes
        {
            get
            {
                return new BlockCipherSpec
                {
                    Algorithm = BlockCipherAgorithm.AES,
                    KeySize = 256,
                    Mode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7,
                    KeyDerivationFunction = KeyDerivationFunction.Guess,
                    InitializationVectorGenerationMode = InitializationVectorGenerationMode.Binary,
                };
            }
        }
        /// <summary>
        /// 3des 3 key PKCS7 ECB
        /// ECB isn't a great idea, but hey, neither is 3des
        /// </summary>
        public static BlockCipherSpec Default3Des
        {
            get
            {
                return new BlockCipherSpec
                {
                    Algorithm = BlockCipherAgorithm.TripleDES,
                    KeySize = 192, //168 used
                    Mode = CipherMode.ECB,
                    PaddingMode = PaddingMode.PKCS7,
                    KeyDerivationFunction = KeyDerivationFunction.Guess,
                    InitializationVectorGenerationMode=InitializationVectorGenerationMode.None,//ecb, n/a
                };
            }
        }
        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>
        /// The algorithm.
        /// </value>
        public BlockCipherAgorithm Algorithm { get; set; }
        /// <summary>
        /// bits
        /// </summary>
        public int KeySize { get; set; }
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public CipherMode Mode { get; set; }
        /// <summary>
        /// Gets or sets the padding mode.
        /// </summary>
        /// <value>
        /// The padding mode.
        /// </value>
        public PaddingMode PaddingMode { get; set; }

        //not configurable for 3des or aes
        //add it if a new algorithm (e.g. rijndael) requires it
        //public int BlockSize { get; set; }
        
        /// <summary>
        /// Gets or sets the key derivation function.
        /// </summary>
        /// <value>
        /// The key derivation function.
        /// </value>
        public KeyDerivationFunction KeyDerivationFunction { get; set; }

        /// <summary>
        /// Gets or sets the initialization vector generation mode.
        /// </summary>
        /// <value>
        /// The initialization vector generation mode.
        /// </value>
        public InitializationVectorGenerationMode InitializationVectorGenerationMode { get; set; }


    }
}
