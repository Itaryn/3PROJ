using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainDemo
{
    public class Transfer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public double Amount { get; set; }
        public double Biscuit { get; set; }

        public Transfer(string fromAddress, string toAddress, double amount, double biscuit)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Biscuit = biscuit;
        }
    }
}
