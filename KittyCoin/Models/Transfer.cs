using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace KittyCoin.Models
{
    /// <summary>
    /// The Transfer class
    /// </summary>
    public class Transfer
    {
        #region Public Attributes

        /// <summary>
        /// The Public address of the sender
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// The Public address of the receiver
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// The amount of the transaction
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
        /// The signature to prove the transfer
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
        /// <remarks>
        /// It's used only for test purpose
        /// The constructor will sign the transfer with the user private information
        /// </remarks>
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
        /// <remarks>
        /// The constructor will sign the transfer with the user private information
        /// </remarks>
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
        /// Check the validity of the transfer :
        /// - If the sender public address is not null
        /// - The amount is more than 0
        /// - The transaction is signed
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ToAddress) &&
                   Amount > 0 && VerifyData();
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
            var tryCount = 0;
            while (tryCount < 100)
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
                    catch (Exception)
                    {
                        tryCount++;
                        Thread.Sleep(10);
                    }
                    finally
                    {
                        rsa.PersistKeyInCsp = false;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 transfers with :
        /// - FromAddress
        /// - ToAddress
        /// - Amount
        /// - Biscuit
        /// - Creation Date
        /// - Signature
        /// </summary>
        /// <param name="obj">
        /// The compared object, transform in Transfer object
        /// </param>
        public override bool Equals(object obj)
        {
            if (!(obj is Transfer other))
                return false;

            return ToString() == other.ToString();
        }

        /// <summary>
        /// The ToString() method
        /// </summary>
        /// <returns>
        /// Concatenate CreationDate, Sender and Receiver address, the amount and biscuit
        /// </returns>
        /// <example>
        /// 03/06/2010 13:52:45 | PublicAddressOfTheSender -> PublicAddressOfTheReceiver : 10 + (0)
        /// </example>
        public override string ToString()
        {
            return $"{CreationDate} | {FromAddress} -> {ToAddress} : {Amount} + ({Biscuit})";
        }

        #endregion
    }
}
