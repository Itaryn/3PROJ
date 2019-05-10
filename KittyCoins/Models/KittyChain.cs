using System;
using System.Collections.Generic;
using System.Linq;
using KittyCoins.Packages;

namespace KittyCoins.Models
{
    /// <summary>
    /// The blockchain class
    /// </summary>
    [Serializable]
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

        public Block FirstBlock => Chain.First(b => string.IsNullOrEmpty(b.PreviousHash));

        public Block LastBlock => GetLastBlock(FirstBlock);

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

        public Block GetLastBlock(Block block)
        {
            if (GetNextBlock(block) == null)
            {
                return block;
            }

            return GetLastBlock(GetNextBlock(block));
        }

        public Block GetNextBlock(Block block)
        {
            return Chain.FirstOrDefault(b => b.PreviousHash.Equals(block.Hash));
        }

        public Block GetPreviousBlock(Block block)
        {
            return Chain.FirstOrDefault(b => b.Hash.Equals(block.PreviousHash));
        }

        public Block GetBlockAt(int id)
        {
            var block = FirstBlock;
            for (int i = 0; i < id; i++)
            {
                if (block == null)
                {
                    return null;
                }

                block = GetNextBlock(block);
            }

            return block;
        }

        /// <summary>
        /// Initialize a new blockchain
        /// </summary>
        public void InitializeChain()
        {
            Chain = new List<Block> { new Block(0, string.Empty, new List<Transfer>(), Difficulty) };
            PendingTransfers = new List<Transfer>();
        }

        /// <summary>
        /// Create a new transfer and add it to the pending list
        /// </summary>
        /// <param name="transfer"></param>
        public string CreateTransfer(Transfer transfer)
        {
            if (new User(Constants.PRIVATE_WORDS_KITTYCHAIN).PublicAddress == transfer.FromAddress ||
                GetBalance(transfer.FromAddress) >= transfer.Amount + transfer.Biscuit)
            {
                PendingTransfers.Add(transfer);
                return "Transfer added";
            }

            return "Error with the transfer. It can't be added (you need more coins)";
        }

        /// <summary>
        /// Add the created block to the blockchain
        /// Create the transfer to give the biscuit to the creator of the block
        /// </summary>
        /// <param name="minerAddress"></param>
        /// <param name="block"></param>
        public string AddBlock(string minerAddress, Block block)
        {
            block.Transfers = PendingTransfers.ToList();
            PendingTransfers = new List<Transfer>();
            block.Index = LastBlock.Index + 1;
            Chain.Add(block);

            if (block.Index % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
            {
                return CheckDifficulty();
            }

            return "";
        }

        /// <summary>
        /// Check the validatity of the blockchain
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var currentBlock = FirstBlock;
            for (var i = 1; i < Chain.Count; i++)
            {
                var nextBlock = GetNextBlock(currentBlock);

                if (currentBlock == null || nextBlock == null ||
                    currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.Hash != nextBlock.PreviousHash ||
                    !currentBlock.Hash.IsLowerHex(currentBlock.Difficulty) ||
                    currentBlock.Transfers.Any(t => !t.IsValid()))
                    return false;

                if (Chain.Count <= i + 1) continue;

                // Verify if the difficulty was well calculated
                if (i % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
                {
                    var compareBlock = Chain.First(block => block.Index.Equals(currentBlock.Index - Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY));

                    var moy = (currentBlock.CreationDate - compareBlock.CreationDate).TotalSeconds /
                              Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY;
                    var pourcentOfDiff = moy / Constants.BLOCK_CREATION_TIME_EXPECTED;

                    if (currentBlock.Difficulty.MultiplyHex(pourcentOfDiff) != nextBlock.Difficulty)
                    {
                        return false;
                    }
                }
                // Verify if the difficulty wasn't calculated when not requested
                else if (currentBlock.Difficulty != nextBlock.Difficulty)
                {
                    return false;
                }

                currentBlock = nextBlock;
            }
            return currentBlock.Hash == currentBlock.CalculateHash();
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

        public string CheckDifficulty()
        {
            var compareBlock = Chain.First(block => block.Index.Equals(Chain.Max(x => x.Index) - Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY));

            var moy = (LastBlock.CreationDate - compareBlock.CreationDate).TotalSeconds /
                      Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY;
            var pourcentOfDiff = moy / Constants.BLOCK_CREATION_TIME_EXPECTED;

            Difficulty = Difficulty.MultiplyHex(pourcentOfDiff);

            if (pourcentOfDiff > 1)
            {
                return $"The difficulty will be down by {Math.Round((pourcentOfDiff - 1) * 100, 2)}%";
            }

            return $"The difficulty will be up by {Math.Round(1 / pourcentOfDiff * 100, 2)}%";
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 blockchain
        /// </summary>
        /// <param name="obj">The compared blockchain</param>
        public override bool Equals(object obj)
        {
            if (!(obj is KittyChain other) ||
                !Chain.SequenceEqual(other.Chain) ||
                !PendingTransfers.SequenceEqual(other.PendingTransfers) ||
                !Difficulty.Equals(other.Difficulty))
                return false;

            return true;
        }

        #endregion
    }
}
