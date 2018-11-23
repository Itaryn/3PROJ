using System;
using System.Collections.Generic;
using System.Linq;

namespace KittyCoins.Models
{
    public class KittyChain
    {
        public IList<Transfer> PendingTransfers;
        public IList<Block> Chain { set;  get; }
        public int Difficulty { set; get; } = 2;
        public Block CurrentMineBlock;

        public KittyChain()
        {
            InitializeChain();
        }
        public KittyChain(IList<Block> chain, IList<Transfer> pendingTransfers)
        {
            Chain = chain;
            PendingTransfers = pendingTransfers;
        }


        public void InitializeChain()
        {
            Chain = new List<Block>();
            PendingTransfers = new List<Transfer>();
            CurrentMineBlock = new Block(0, DateTime.Now, null, PendingTransfers);
        }
        
        public Block GetLatestBlock()
        {
            return Chain.Last();
        }

        public void CreateTransfer(Transfer transfer)
        {
            PendingTransfers.Add(transfer);
        }

        public void AddBlock(string minerAdress, Block block)
        {
            var latestBlock = GetLatestBlock();
            var block = new Block(latestBlock.Index + 1, DateTime.Now, latestBlock.Hash, PendingTransfers);
            AddBlock(block);

            PendingTransfers = new List<Transfer>();
            CreateTransfer(new Transfer(null, minerAddress, block.Transfers.Sum(transfer => transfer.Biscuit), 0));
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
                foreach (var transfer in block.Transfers)
                {
                    if (transfer.FromAddress == address)
                    {
                        balance -= transfer.Amount;
                        balance -= transfer.Biscuit;
                    }

                    if (transfer.ToAddress == address)
                        balance += transfer.Amount;
                }

            return balance;
        }

        public override bool Equals(object obj)
        {
            KittyChain compareChain;
            try { compareChain = (KittyChain)obj; }
            catch (Exception) { return false; }
            if (compareChain == null) return false;

            return KittyChain.Equals(compareChain.KittyChain) &&
                   PendingTransfers.Equals(compareChain.PendingTransfers);
        }
    }
}
