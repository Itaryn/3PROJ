namespace KittyCoins.Models
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// The transfer class
    /// </summary>
    public class Transfer
    {
        #region Public Attributes

        /// <summary>
        /// The adress of the sender
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// The address of the receiver
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// The amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The amount for miner
        /// </summary>
        public double Biscuit { get; set; }

        /// <summary>
        /// The creation date of the transfer
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The signature to proove the transfer
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor to deserialize with NewtonSoft
        /// </summary>
        public Transfer() { }

        /// <summary>
        /// Constructor to copy a transfer already create
        /// </summary>
        /// <param name="fromUser"></param>
        /// <param name="toAddress"></param>
        /// <param name="amount"></param>
        /// <param name="biscuit"></param>
        /// <param name="creationDate"></param>
        public Transfer(User fromUser, string toAddress, double amount, double biscuit, DateTime creationDate)
        {
            FromAddress = fromUser.PublicAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = creationDate;
            SignData(fromUser);
        }

        /// <summary>
        /// Constructor to create a new transfer
        /// </summary>
        /// <param name="fromUser"></param>
        /// <param name="toAddress"></param>
        /// <param name="amount"></param>
        /// <param name="biscuit"></param>
        public Transfer(User fromUser, string toAddress, double amount, double biscuit)
        {
            FromAddress = fromUser.PublicAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = DateTime.UtcNow;
            SignData(fromUser);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check the validity of the transfer
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ToAddress) &&
                   Amount > 0 &&
                   VerifyData();
        }

        /// <summary>
        /// Calculate the Hash of the transfer
        /// </summary>
        /// <returns></returns>
        public string ToHash()
        {
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes($"{FromAddress}-{ToAddress}-{Amount}-{Biscuit}-{CreationDate}"))
                    .Select(item => item.ToString("x2")));
            }
        }

        /// <summary>
        /// Sign the data with the user information
        /// </summary>
        /// <param name="user"></param>
        public void SignData(User user)
        {
            Signature = user.SignData(ToHash());
        }

        /// <summary>
        /// Verify the signature of the transfer
        /// </summary>
        /// <returns></returns>
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

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 transfer
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool Equals(Transfer other)
        {
            return string.Equals(FromAddress, other.FromAddress) &&
                   string.Equals(ToAddress, other.ToAddress) &&
                   Amount.Equals(other.Amount) &&
                   Biscuit.Equals(other.Biscuit) &&
                   CreationDate.Equals(other.CreationDate) &&
                   string.Equals(Signature, other.Signature);
        }

        /// <summary>
        /// The ToString() Method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{CreationDate} | {FromAddress} -> {ToAddress} : {Amount} + ({Biscuit})";
        }

        #endregion
    }
}
