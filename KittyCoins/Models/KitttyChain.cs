namespace KittyCoins.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    

    public class KittyChain
    {
        public List<Transfer> PendingTransfers { get; set; }
        public List<Block> Chain { set;  get; }
        public int Difficulty { set; get; } = 2;
        public double Biscuit { set; get; } = 10;
        public Block CurrentMineBlock { get; set; }

        public KittyChain() { }
        public KittyChain(List<Block> chain, List<Transfer> pendingTransfers)
        {
            if (chain != null)
            {
                Chain = chain;
                PendingTransfers = pendingTransfers;
                CurrentMineBlock = new Block(chain.Count, Chain.First().Hash, PendingTransfers);
            }
            else
            {
                InitializeChain();
            }
        }
        
        public void InitializeChain()
        {
            Chain = new List<Block> {new Block(0, string.Empty, new List<Transfer>())};
            PendingTransfers = new List<Transfer>();
            CurrentMineBlock = new Block(1, Chain.First().Hash, PendingTransfers);
        }

        public void CreateTransfer(Transfer transfer)
        {
            if (GetBalance(transfer.FromAddress) >= transfer.Amount + transfer.Biscuit)
                PendingTransfers.Add(transfer);
        }

        public void AddBlock(string minerAddress, Block block)
        {
            Chain.Add(block);
            PendingTransfers = new List<Transfer>();
            CreateTransfer(new Transfer(null, minerAddress, Biscuit, 0));
        }

        public bool IsValid()
        {
            for (var i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.PreviousHash != previousBlock.Hash || 
                    currentBlock.Transfers.Any(t => !t.IsValid()))
                    return false;
            }
            return true;
        }

        public double GetBalance(string address)
        {
            double balance = 0;

            foreach (var block in Chain)
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
        
        protected bool Equals(KittyChain other)
        {
            return Chain.Equals(other.Chain) &&
                       PendingTransfers.Equals(other.PendingTransfers);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
