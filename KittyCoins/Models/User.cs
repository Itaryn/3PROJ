namespace KittyCoins.Models
{
    using System;
    using System.Security.Cryptography;
    using ViewModels;

    public class User
    {
        private readonly RSAParameters _privateKey;
        public string PublicAddress;

        public User(RSAParameters privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);

                _privateKey = privateKey;
                PublicAddress = Convert.ToBase64String(rsa.ExportParameters(includePrivateParameters: false).Modulus);
            }
        }

        public User(string privateWords)
        {
            using (var rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = privateWords }))
            {
                _privateKey = rsa.ExportParameters(includePrivateParameters: true);
                PublicAddress = Convert.ToBase64String(rsa.ExportParameters(includePrivateParameters: false).Modulus);
            }
        }

        public double GetBalance()
        {
            return MainViewModel.BlockChain.GetBalance(PublicAddress);
        }

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
    }
}