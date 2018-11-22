using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockchainDemo
{
    public class Blockchain
    {
        public IList<Transfer> PendingTransfers = new List<Transfer>();
        public IList<Block> KittyChain { set;  get; }
        public int Difficulty { set; get; } = 2;

        public Blockchain()
        {
            KittyChain = new List<Block>();
            PendingTransfers = new List<Transfer>();
        }
        public Blockchain(List<Block> kittyChain, List<Transfer> pendingTransfers)
        {
            KittyChain = kittyChain;
            PendingTransfers = pendingTransfers;
        }


        public void InitializeChain()
        {
            KittyChain = new List<Block>();
            var block = new Block(DateTime.Now, null, PendingTransfers);
            PendingTransfers = new List<Transfer>();
            AddBlock(block);
        }
        
        public Block GetLatestBlock()
        {
            return KittyChain.Last();
        }

        public void CreateTransaction(Transfer transaction)
        {
            PendingTransfers.Add(transaction);
        }
        public void ProcessPendingTransactions(string minerAddress)
        {
            var latestBlock = GetLatestBlock();
            var block = new Block(DateTime.Now, latestBlock.Hash, PendingTransfers, latestBlock.Index + 1);
            AddBlock(block);

            PendingTransfers = new List<Transfer>();
            CreateTransaction(new Transfer(null, minerAddress, block.Transfers.Sum(transfer => transfer.Biscuit), 0));
        }

        public void AddBlock(Block block)
        {
            block.Mine(Difficulty);
            KittyChain.Add(block);
        }

        public bool IsValid()
        {
            for (var i = 1; i < KittyChain.Count; i++)
            {
                var currentBlock = KittyChain[i];
                var previousBlock = KittyChain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
            }
            return true;
        }

        public double GetBalance(string address)
        {
            double balance = 0;

            foreach (var block in KittyChain)
                foreach (var transaction in block.Transfers)
                {
                    if (transaction.FromAddress == address)
                    {
                        balance -= transaction.Amount;
                        balance -= transaction.Biscuit;
                    }

                    if (transaction.ToAddress == address)
                        balance += transaction.Amount;
                }

            return balance;
        }
    }
}
