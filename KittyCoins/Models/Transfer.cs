using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace KittyCoins.Models
{
    public class Transfer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public double Amount { get; set; }
        public double Biscuit { get; set; }
        public DateTime CreationDate { get; set; }
        public string Signature { get; set; }

        public Transfer(string fromAddress, string toAddress, double amount, double biscuit, DateTime creationDate, RSAParameters privateKey)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = creationDate;
            SignData(privateKey);
        }
        public Transfer(string fromAddress, string toAddress, double amount, double biscuit, RSAParameters privateKey)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = DateTime.UtcNow;
            SignData(privateKey);
        }

        protected bool Equals(Transfer other)
        {
            return string.Equals(FromAddress, other.FromAddress) &&
                   string.Equals(ToAddress, other.ToAddress) &&
                   Amount.Equals(other.Amount) &&
                   Biscuit.Equals(other.Biscuit) &&
                   CreationDate.Equals(other.CreationDate) &&
                   string.Equals(Signature, other.Signature);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ToAddress) &&
                   Amount > 0;
        }

        public override string ToString()
        {
            return $"{CreationDate} | {FromAddress} -> {ToAddress} : {Amount} + ({Biscuit})";
        }

        public string ToHash()
        {
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes($"{FromAddress}-{ToAddress}-{Amount}-{Biscuit}-{CreationDate}"))
                    .Select(item => item.ToString("x2")));
            }
        }

        public void SignData(RSAParameters privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try {
                    rsa.ImportParameters(privateKey);

                    Signature = Convert.ToBase64String(rsa.SignData(Convert.FromBase64String(ToHash()), CryptoConfig.MapNameToOID("SHA256")));
                }
                catch (CryptographicException) {
                }
                finally {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
        public bool VerifyData()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    var rsap = new RSAParameters
                    {
                        Modulus = Convert.FromBase64String(FromAddress),
                        Exponent = Convert.FromBase64String("AQAB")
                    };
                    rsa.ImportParameters(rsap);
                    
                    return rsa.VerifyData(Convert.FromBase64String(ToHash()), CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(Signature));
                }
                catch (CryptographicException)
                {
                    return false;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
    }
}
