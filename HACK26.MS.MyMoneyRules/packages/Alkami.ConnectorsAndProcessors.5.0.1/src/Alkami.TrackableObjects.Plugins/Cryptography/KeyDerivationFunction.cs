// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="KeyDerivationFunction.cs" company="Alkami Technology, Inc.">
// //   Copyright 2013 Alkami Technology, Inc.  All rights reserved.
// // </copyright>
// // <summary>
// //
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
namespace Alkami.TrackableObjects.Plugins.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public enum KeyDerivationFunction
    {
        /// <summary>
        /// The guess
        /// </summary>
        Guess=0,
        /// <summary>
        /// The raw ut f8 string
        /// </summary>
        RawUtf8String,
        /// <summary>
        /// The base64 string
        /// </summary>
        Base64String,
        /// <summary>
        /// The raw bytes
        /// </summary>
        RawBytes,
    }
}