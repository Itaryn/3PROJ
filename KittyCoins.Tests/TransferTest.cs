namespace KittyCoins.Tests
{
    using KittyCoins.Models;
    using System;
    using System.Security.Cryptography;
    using Xunit;

    public class TransferTest
    {
        [Fact]
        public void CreateTransferTest()
        {
            var rsaUser2 = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = "temperature lesson reptile impress memorandum side skeleton marketing bill storage" });

            var user1 = new User("arch spill trousers slap spiteful fanatical fluffy man part sheet");
            var user2 = new User(rsaUser2.ExportParameters(true));

            var transfer1 = new Transfer(user1, user2.PublicAddress, 10, 0, new DateTime(2018, 11, 3, 15, 26, 53));
            var transfer2 = new Transfer(user2, user1.PublicAddress, 10, 0);
            
            Assert.Equal("AQAB", Convert.ToBase64String(rsaUser2.ExportParameters(false).Exponent));
            
            Assert.Equal("8Y/Ob/cc3VsQEdxN5/OB+1T5kobV2uGLJq0re86vRpFui/S9j8cT3iUANwxJX2S6FTQlTvbp6O2spoDXKeK1kJNsf4QwXA5VJPeJ9LUgHmja5MSYe7MugXRZ10zvzun1n58Z4KFTfqxYas1X13YNOrrqyurmAprx0Q8LBnXpO3U=", user1.PublicAddress);
            Assert.Equal("pL6G8f6+EJn9B9MqdsjpkChbNZIcnotztmgRBCykWpHSKH4R6eNmA5x3T46/ggOLpx4U6DmUMf8BhkuLiAq3S/aI7EeWuEwwRfKMe+XL55H+gPlqL12472bysayfxoOjbG+V1cMDsxC9+yvRLklEjuvTpQ9a5ZdlJzt6IrBWhnk=", user2.PublicAddress);

            Assert.True(transfer1.VerifyData());
            Assert.True(transfer2.VerifyData());
            Assert.False(transfer1.Equals(transfer2));

            Assert.Equal($"03/11/2018 15:26:53 | {user1.PublicAddress} -> {user2.PublicAddress} : 10 + (0)", transfer1.ToString());
        }
    }
}
