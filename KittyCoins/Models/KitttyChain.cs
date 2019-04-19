namespace KittyCoins.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    
    /// <summary>
    /// The blockchain class
    /// </summary>
    public class KittyChain
    {
        #region Public Attributes

        /// <summary>
        /// List of transfers that is pending the block to be created
        /// </summary>
        public List<Transfer> PendingTransfers { get; set; }

        /// <summary>
        /// List of the blocks in the chain
        /// </summary>
        public List<Block> Chain { set; get; }

        /// <summary>
        /// Actual difficulty of the blockchain
        /// Depending of the average creation time of a block
        /// </summary>
        public int Difficulty { set; get; } = 2;

        /// <summary>
        /// Numbler of KittyCoin given to the creator of a block
        /// </summary>
        public double Biscuit { set; get; } = 10;

        /// <summary>
        /// The block in creation
        /// </summary>
        public Block CurrentMineBlock { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor to deserialize with NewtonSoft
        /// </summary>
        public KittyChain() { }

        /// <summary>
        /// Constructor who can initialized or get a chain/pendingTransfers in parameter
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="pendingTransfers"></param>
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize a new blockchain
        /// </summary>
        public void InitializeChain()
        {
            Chain = new List<Block> { new Block(0, string.Empty, new List<Transfer>()) };
            PendingTransfers = new List<Transfer>();
            CurrentMineBlock = new Block(1, Chain.First().Hash, PendingTransfers);
        }

        /// <summary>
        /// Create a new transfer and add it to the pending list
        /// </summary>
        /// <param name="transfer"></param>
        public void CreateTransfer(Transfer transfer)
        {
            if (GetBalance(transfer.FromAddress) >= transfer.Amount + transfer.Biscuit)
                PendingTransfers.Add(transfer);
        }

        /// <summary>
        /// Add the created block to the blockchain
        /// Create the transfer to give the biscuit to the creator of the block
        /// </summary>
        /// <param name="minerAddress"></param>
        /// <param name="block"></param>
        public void AddBlock(string minerAddress, Block block)
        {
            Chain.Add(block);
            PendingTransfers = new List<Transfer>();
            CreateTransfer(new Transfer(null, minerAddress, Biscuit, 0));
        }

        /// <summary>
        /// Check the validatity of the blockchain
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            for (var i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get the balance of an address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 blockchain
        /// </summary>
        /// <param name="other">The compared block</param>
        protected bool Equals(KittyChain other)
        {
            return Chain.Equals(other.Chain) &&
                   PendingTransfers.Equals(other.PendingTransfers);
        }

        /// <summary>
        /// The ToString() method
        /// </summary>
        /// <returns>
        /// The Json object
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}
