using System;
using System.Security.Cryptography;
using KittyCoins.Models;
using Xunit;

namespace UnitTestKittyCoins
{
    public class TransferTest
    {
        [Fact]
        public void CreateTransferTest()
        {
            var cspParams = new CspParameters { KeyContainerName = "arch spill trousers slap spiteful fanatical fluffy man part sheet" };
            var rsaUser1 = new RSACryptoServiceProvider();
            var rsaUser2 = new RSACryptoServiceProvider(cspParams);

            var privateKeyUser1 = rsaUser1.ExportParameters(true);
            var publicKeyUser1 = rsaUser1.ExportParameters(false);
            var privateKeyUser2 = rsaUser2.ExportParameters(true);
            var publicKeyUser2 = rsaUser2.ExportParameters(false);

            var transfer1 = new Transfer(Convert.ToBase64String(publicKeyUser1.Modulus), Convert.ToBase64String(publicKeyUser2.Modulus), 10, 0, new DateTime(2018, 11, 3, 15, 26, 53), privateKeyUser1);
            var transfer2 = new Transfer(Convert.ToBase64String(publicKeyUser2.Modulus), Convert.ToBase64String(publicKeyUser1.Modulus), 10, 0, privateKeyUser2);

            Assert.Equal("AQAB", Convert.ToBase64String(publicKeyUser1.Exponent));
            Assert.Equal("AQAB", Convert.ToBase64String(publicKeyUser2.Exponent));
            
            Assert.Equal("9CQNmWPR6gZigIg2xg0qN8IARnVHrPRowSj/uQ6gEAETNyqRXXq1PDHaCs38FPS+C3OovssWr6lpgUtry89P2UjDrgdbzvpqM3fcltxt1noGZfKpxr4mEkD4Duddbe8NTyalPiOUgLsSumE9ELc+lsHovvtviXI015MM9GQ9nis=", Convert.ToBase64String(publicKeyUser2.Modulus));

            Assert.True(transfer1.VerifyData());
            Assert.True(transfer2.VerifyData());
            Assert.False(transfer1.Equals(transfer2));

            Assert.Equal($"03/11/2018 15:26:53 | {Convert.ToBase64String(publicKeyUser1.Modulus)} -> {Convert.ToBase64String(publicKeyUser2.Modulus)} : 10 + (0)", transfer1.ToString());
        }
    }
}
