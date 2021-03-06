using System;
using System.Collections.Generic;
using KittyCoin.Models;
using Xunit;

namespace KittyCoin.Tests
{
    public class BlockTest
    {
        [Fact]
        public void CreateBlockTest()
        {
            var block1 = new Block(0, "", new DateTime(2018, 12, 1, 15, 23, 3), string.Empty, new List<Transfer>(), "");
            var block2 = new Block(1, "", block1.Hash, new List<Transfer>(), "");

            Assert.False(block1.Equals(block2));
            Assert.Equal("0 (01/12/2018 15:23:03) | 0 transfers | 0 coins", block1.ToString());
        }
    }
}
