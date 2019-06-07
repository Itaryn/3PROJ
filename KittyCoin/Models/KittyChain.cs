using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KittyCoin.Packages;
using KittyCoin.ViewModels;
using Newtonsoft.Json;

namespace KittyCoin.Models
{
    /// <summary>
    /// The Blockchain class
    /// </summary>
    [Serializable]
    public class KittyChain
    {
        #region Public Attributes

        /// <summary>
        /// List of transaction that is pending the block to be created
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
        /// Number of KittyCoin given to the creator of a block
        /// </summary>
        public double Biscuit { set; get; } = 10;

        /// <summary>
        /// Return the first block of the chain
        /// Search the block who have a null PreviousHash
        /// </summary>
        public Block FirstBlock => Chain.First(b => string.IsNullOrEmpty(b.PreviousHash));

        /// <summary>
        /// Return the last block of the chain
        /// Begin with the first block and search the next one with his Hash
        /// </summary>
        public Block LastBlock => GetLastBlock(FirstBlock);

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor to deserialize with NewtonSoft
        /// </summary>
        public KittyChain() { }

        /// <summary>
        /// Constructor who can initialize or get a chain/pendingTransfers in parameter
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
        /// Get the last block of the chain, begin with one block (most of the time the first one)
        /// </summary>
        /// <param name="block"></param>
        /// <returns>
        /// The Last block of the chain
        /// </returns>
        public Block GetLastBlock(Block block)
        {
            if (GetNextBlock(block) == null)
            {
                return block;
            }

            return GetLastBlock(GetNextBlock(block));
        }

        /// <summary>
        /// Get the next block of a block (use hash and previous hash)
        /// </summary>
        /// <param name="block"></param>
        /// <returns>
        /// The next block of the block in parameter
        /// </returns>
        public Block GetNextBlock(Block block)
        {
            return Chain.FirstOrDefault(b => b.PreviousHash.Equals(block?.Hash));
        }

        /// <summary>
        /// Get the previous block of a block (use hash and previous hash)
        /// </summary>
        /// <param name="block"></param>
        /// <returns>
        /// The previous block of the block in parameter
        /// </returns>
        public Block GetPreviousBlock(Block block)
        {
            return Chain.FirstOrDefault(b => b.Hash.Equals(block.PreviousHash));
        }

        /// <summary>
        /// Get the block at a specific position
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The block at the specific position given in parameter
        /// </returns>
        /// <remarks>
        /// Used in the difficulty check
        /// </remarks>
        public Block GetBlockAt(int id)
        {
            var block = FirstBlock;
            for (var i = 0; i < id; i++)
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
            Chain = new List<Block> { new Block(0, "", string.Empty, new List<Transfer>(), Difficulty) };
            PendingTransfers = new List<Transfer>();

            var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
            if (receivers != null)
            {
                foreach (EventHandler receiver in receivers)
                {
                    receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                }
            }
        }

        /// <summary>
        /// Create a new transfer and add it to the pending list
        /// </summary>
        /// <param name="transfer"></param>
        public string CreateTransfer(Transfer transfer)
        {
            if (transfer == null ||
                transfer.Amount <= 0 ||
                transfer.Biscuit < 0)
            {
                return "Error with the transfer. It can't be added (You don't have enough money or you're trying to send negative/zero value)";
            }

            var guid = Guid.NewGuid();
            MainViewModel.WaitingForBlockchainAccess(guid);

            var chain = MainViewModel.BlockChain.Chain.ToArray();
            var transactions = MainViewModel.BlockChain.PendingTransfers.ToArray();

            MainViewModel.BlockChainWaitingList.Remove(guid);

            if (GetBalance(transfer.FromAddress, chain, transactions) >= transfer.Amount + transfer.Biscuit ||
                new User(Constants.PRIVATE_WORDS_KITTYCHAIN).PublicAddress == transfer.FromAddress)
            {
                PendingTransfers.Add(transfer);
                var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
                if (receivers != null)
                {
                    foreach (EventHandler receiver in receivers)
                    {
                        receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                    }
                }
                return Constants.TRANSFER_ADDED;
            }

            return "Error with the transfer. It can't be added (You don't have enough money or you're trying to send negative/zero value)";
        }

        /// <summary>
        /// Add the created block to the blockchain
        /// Create the transfer to give the biscuit to the creator of the block
        /// </summary>
        /// <param name="block"></param>
        public string AddBlock(Block block)
        {
            block.Transfers = PendingTransfers.ToArray();
            PendingTransfers = new List<Transfer>();
            block.Index = LastBlock.Index + 1;
            Chain.Add(block);

            if (block.Index % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
            {
                return CheckDifficulty();
            }

            var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
            if (receivers != null)
            {
                foreach (EventHandler receiver in receivers)
                {
                    receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                }
            }

            return "";
        }

        /// <summary>
        /// Check the validatity of the blockchain
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It will check :
        /// - Hash
        /// - Previous Hash
        /// - Transaction validity
        /// - Hash is under difficulty
        /// - Well calculated difficulty
        /// </remarks>
        public bool IsValid()
        {
            var currentBlock = GetNextBlock(FirstBlock);
            for (var i = 1; i < Chain.Count - 1; i++)
            {
                var nextBlock = GetNextBlock(currentBlock);

                if (currentBlock == null || nextBlock == null ||
                    currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.Hash != nextBlock.PreviousHash ||
                    !currentBlock.Hash.IsLowerHex(currentBlock.Difficulty) ||
                    currentBlock.Transfers.Any(t => !t.IsValid()))
                {
                    return false;
                }

                // Verify if the difficulty was well calculated
                if (i % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
                {
                    var compareBlock = GetBlockAt(i + 1 - Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY);

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

            if (Chain.Count == 1) return true;

            return currentBlock.Hash == currentBlock.CalculateHash();
        }

        /// <summary>
        /// Get the balance of an address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="chain"></param>
        /// <param name="transactions"></param>
        /// <returns>
        /// The amount of coin
        /// </returns>
        public double GetBalance(string address, Block[] chain, Transfer[] transactions)
        {
            double balance = 0;

            // Check transactions in blocks
            foreach (var block in chain)
            {
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
            }

            // Check transactions waiting
            foreach (var transfer in transactions)
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

        /// <summary>
        /// Get all transactions of an address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<Transfer> GetTransactions(string address)
        {
            var transactions = new List<Transfer>();

            var guid = Guid.NewGuid();
            MainViewModel.WaitingForBlockchainAccess(guid);

            var chain = Chain.ToList();

            MainViewModel.BlockChainWaitingList.Remove(guid);

            foreach (var block in chain)
            {
                foreach (var transfer in block.Transfers)
                {
                    if (transfer.FromAddress == address ||
                        transfer.ToAddress == address)
                    {
                        transactions.Add(transfer);
                    }
                }
            }

            return transactions;
        }

        /// <summary>
        /// Calculate the difficulty from the mean block creation time
        /// </summary>
        /// <returns>
        /// The sentence displayed to the user
        /// </returns>
        public string CheckDifficulty()
        {
            var compareBlock = GetBlockAt(Chain.Count - Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY);

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

        /// <summary>
        /// Save the blockchain to the txt file
        /// </summary>
        public string SaveBlockChain()
        {
            try
            {
                File.WriteAllText(Constants.SAVE_FILENAME, JsonConvert.SerializeObject(this));
                return "Blockchain saved";
            }
            catch (Exception e)
            {
                return $"Can't save the blockchain into the file, error : {e.Message}";
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 blocks with :
        /// - Chain
        /// - PendingTransfers
        /// - Difficulty
        /// </summary>
        /// <param name="obj">
        /// The compared object, transform in KittyChain object
        /// </param>
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
