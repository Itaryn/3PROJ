using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using KittyCoins.Models;
using Xunit;

namespace UnitTestKittyCoins
{
    public class KittyChainTest
    {
        [Fact]
        public void CreateKittyChainTest()
        {
            var blockchain = new KittyChain();
            
            Assert.Single(blockchain.Chain);

            var blockchain2 = new KittyChain(getBlocks(), new List<Transfer>());

            Assert.True(blockchain2.IsValid());
        }

        private List<Block> getBlocks()
        {
            var block1 = new Block(0, new DateTime(2018, 10, 1, 8, 0, 0), string.Empty, new List<Transfer>());
            var block2 = new Block(1, new DateTime(2018, 10, 1, 8, 10, 0), block1.Hash, new List<Transfer>());
            var block3 = new Block(2, new DateTime(2018, 10, 1, 8, 20, 0), block2.Hash, new List<Transfer>());
            var block4 = new Block(3, new DateTime(2018, 10, 1, 8, 30, 0), block3.Hash, new List<Transfer>());

            return new List<Block>
            {
                block1,
                block2,
                block3,
                block4
            };
        }
    }
}
