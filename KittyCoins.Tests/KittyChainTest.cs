using System;
using System.Collections.Generic;
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
            blockchain.InitializeChain();

            Assert.Single(blockchain.Chain);

            var blockchain2 = new KittyChain(GetBlocks(), new List<Transfer>());

            Assert.True(blockchain2.IsValid());
        }

        [Fact]
        public void GetBalanceTest()
        {
            var blockchain2 = new KittyChain(GetBlocks(), new List<Transfer>());

            var user1 = new User("a");
            var user2 = new User("b");
            var user3 = new User("c");

            Assert.Equal(30, blockchain2.GetBalance(user1.PublicAddress));
            Assert.Equal(-65, blockchain2.GetBalance(user2.PublicAddress));
            Assert.Equal(10, blockchain2.GetBalance(user3.PublicAddress));
        }

        private List<Block> GetBlocks()
        {
            var user1 = new User("a");
            var user2 = new User("b");
            var user3 = new User("c");

            var transfer1 = new List<Transfer>
            {
                new Transfer(user2, user1.PublicAddress, 50, 10),
                new Transfer(user2, user1.PublicAddress, 44, 5),
                new Transfer(user1, user2.PublicAddress, 50, 4)
            };
            var transfer2 = new List<Transfer>
            {
                new Transfer(user2, user1.PublicAddress, 5, 1),
                new Transfer(user1, user3.PublicAddress, 10, 5)
            };

            var block1 = new Block(0, "", new DateTime(2018, 10, 1, 8, 0, 0), string.Empty, transfer1, new string('F', 64));
            var block2 = new Block(1, "", new DateTime(2018, 10, 1, 8, 10, 0), block1.Hash, transfer2, new string('F', 64));
            var block3 = new Block(2, "", new DateTime(2018, 10, 1, 8, 20, 0), block2.Hash, new List<Transfer>(), new string('F', 64));
            var block4 = new Block(3, "", new DateTime(2018, 10, 1, 8, 30, 0), block3.Hash, new List<Transfer>(), new string('F', 64));

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
