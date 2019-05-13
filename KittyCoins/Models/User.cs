using System;
using System.Security.Cryptography;

namespace KittyCoins.Models
{
    /// <summary>
    /// The user class
    /// </summary>
    public class User
    {
        #region Private Attributes

        /// <summary>
        /// The private key
        /// </summary>
        private readonly RSAParameters _privateKey;

        #endregion

        #region Public Attributes

        /// <summary>
        /// The public address
        /// </summary>
        public string PublicAddress;

        #endregion

        #region Constructors

        /// <summary>
        /// Create the user with a private key
        /// </summary>
        /// <param name="privateKey"></param>
        public User(RSAParameters privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);

                _privateKey = privateKey;
                PublicAddress = Convert.ToBase64String(rsa.ExportParameters(includePrivateParameters: false).Modulus);
            }
        }

        /// <summary>
        /// Create the user with a list of word
        /// </summary>
        /// <param name="privateWords"></param>
        public User(string privateWords)
        {
            using (var rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = privateWords }))
            {
                _privateKey = rsa.ExportParameters(includePrivateParameters: true);
                PublicAddress = Convert.ToBase64String(rsa.ExportParameters(includePrivateParameters: false).Modulus);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sign the data in parameter
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SignData(string data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.ImportParameters(_privateKey);

                    return Convert.ToBase64String(rsa.SignData(Convert.FromBase64String(data), CryptoConfig.MapNameToOID("SHA256")));
                }
                catch (CryptographicException)
                {
                    return "";
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        #endregion
    }
}