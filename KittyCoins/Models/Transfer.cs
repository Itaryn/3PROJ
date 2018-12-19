using System;
using System.Security.Policy;

namespace KittyCoins.Models
{
    public class Transfer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public double Amount { get; set; }
        public double Biscuit { get; set; }
        public DateTime CreationDate { get; set; }

        public Transfer(string fromAddress, string toAddress, double amount, double biscuit)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
            CreationDate = DateTime.UtcNow;
        }

        public override bool Equals(object obj)
        {
            Transfer compareTransfer;
            try { compareTransfer = (Transfer)obj; }
            catch (Exception) { return false; }
            if (compareTransfer == null) return false;

            return FromAddress.Equals(compareTransfer.FromAddress) &&
                   ToAddress.Equals(compareTransfer.ToAddress) &&
                   Amount.Equals(compareTransfer.Amount) &&
                   Biscuit.Equals(compareTransfer.Biscuit) &&
                   CreationDate.Equals(compareTransfer.CreationDate);
        }
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ToAddress) &&
                   Amount > 0;
        }
    }
}
