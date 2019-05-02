using System;
using KittyCoins.Packages;
using KittyCoins.ViewModels;

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
        public string Difficulty { set; get; } = "0001000000000000000000000000000000000000000000000000000000000000";

        /// <summary>
        /// Numbler of KittyCoin given to the creator of a block
        /// </summary>
        public double Biscuit { set; get; } = 10;

        public Block LastBlock => Chain.First(block => block.Index.Equals(Chain.Max(x => x.Index)));

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
            Chain = new List<Block> { new Block(0, string.Empty, new List<Transfer>(), MainViewModel.BlockChain.Difficulty) };
            PendingTransfers = new List<Transfer>();
        }

        /// <summary>
        /// Create a new transfer and add it to the pending list
        /// </summary>
        /// <param name="transfer"></param>
        public void CreateTransfer(Transfer transfer)
        {
            if (new User(Constants.PRIVATE_WORDS_KITTYCHAIN).PublicAddress == transfer.FromAddress ||
                GetBalance(transfer.FromAddress) >= transfer.Amount + transfer.Biscuit)
            {
                PendingTransfers.Add(transfer);
                MainViewModel.Client.NewTransfer(transfer);
            }
            else
            {
                MainViewModel.MessageFromClientOrServer.Add("Error with the transfer. It can't be added (you need more coins)");
            }
        }

        /// <summary>
        /// Add the created block to the blockchain
        /// Create the transfer to give the biscuit to the creator of the block
        /// </summary>
        /// <param name="minerAddress"></param>
        /// <param name="block"></param>
        public void AddBlock(string minerAddress, Block block)
        {
            block.Transfers = PendingTransfers.ToList();
            PendingTransfers = new List<Transfer>();
            block.Index = LastBlock.Index + 1;
            Chain.Add(block);

            if (block.Index % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
            {
                MainViewModel.BlockChain.CheckDifficulty();
            }

            CreateTransfer(new Transfer(new User(Constants.PRIVATE_WORDS_KITTYCHAIN), minerAddress, Biscuit, 0));
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

        public void CheckDifficulty()
        {
            var compareBlock = Chain.First(block => block.Index.Equals(Chain.Max(x => x.Index) - Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY));

            var moy = (LastBlock.CreationDate - compareBlock.CreationDate).TotalSeconds /
                      Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY;
            var pourcentOfDiff = moy / Constants.BLOCK_CREATION_TIME_EXPECTED;

            if (pourcentOfDiff > 1)
            {
                MainViewModel.MessageFromClientOrServer.Add($"The difficulty will be down by {Math.Round(pourcentOfDiff * 100, 2)}%");
            }
            else
            {
                MainViewModel.MessageFromClientOrServer.Add($"The difficulty will be up by {Math.Round(1 / pourcentOfDiff * 100, 2)}%");
            }

            Difficulty = Difficulty.MultiplyHex(pourcentOfDiff);
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 blockchain
        /// </summary>
        /// <param name="obj">The compared blockchain</param>
        public override bool Equals(object obj)
        {
            if (!(obj is KittyChain other))
                return false;

            return string.Equals(JsonConvert.SerializeObject(this), JsonConvert.SerializeObject(other));
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
