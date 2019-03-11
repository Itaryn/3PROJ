namespace KittyCoins.Models
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    [Serializable]
    public class Transfer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public double Amount { get; set; }
        public double Biscuit { get; set; }
        public DateTime CreationDate { get; set; }
        public string Signature { get; set; }

        public Transfer() { }

        public Transfer(User fromUser, string toAddress, double amount, double biscuit, DateTime creationDate)
        {
            FromAddress = fromUser.PublicAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = creationDate;
            SignData(fromUser);
        }
        public Transfer(User fromUser, string toAddress, double amount, double biscuit)
        {
            FromAddress = fromUser.PublicAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = DateTime.UtcNow;
            SignData(fromUser);
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

        public void SignData(User user)
        {
            Signature = user.SignData(ToHash());
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
