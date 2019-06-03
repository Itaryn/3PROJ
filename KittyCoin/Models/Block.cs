using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KittyCoin.Packages;
using KittyCoin.ViewModels;
using Newtonsoft.Json;

namespace KittyCoin.Models
{
    /// <summary>
    /// The Block Class
    /// </summary>
    public class Block
    {
        #region Public Attributes

        /// <summary>
        /// Index of the Block
        /// </summary>
        /// <example>42</example>
        public int Index { get; set; }

        /// <summary>
        /// The Public address of the creator of the block
        /// </summary>
        /// <example>
        /// The Public Address used to give money to the miner
        /// pUSc7D+6oB/BeZEw5N2O21c9AyuryRGRcInNP++sWUbeIOW+lucNeR9jqk6kwyxHZ0iZvEcR/eJUo1DPzeXyco0jA5XB/7NnWBCrCX/omJ34NdnYyCQblver3JPhqTyzEn8Wt0YXm3U3OwcxorixfSDWH841gliCS4Z8MJ4u/JE=
        /// </example>
        public string Owner { get; set; }

        /// <summary>
        /// Creation DateTime of the Block
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The Hash of the previous block
        /// </summary>
        public string PreviousHash { get; set; }

        /// <summary>
        /// List of transactions contained in the block
        /// </summary>
        public Transfer[] Transfers { get; set; }

        /// <summary>
        /// A random Guid
        /// </summary>
        /// <remarks>
        /// It will be used when calculating the hash of the block
        /// </remarks>
        /// <example>
        /// 0cf93ebb-4d54-4905-879b-dcbf1019d093
        /// </example>
        public Guid Guid { get; set; }

        /// <summary>
        /// The Hash of the block
        /// </summary>
        /// <example>
        /// 00000C4B3B74BC3008C97AC06C29BC7F7BB390009FEC257934E13631ECB1180D
        /// </example>
        public string Hash { get; set; }

        /// <summary>
        /// The difficulty of the BlockChain when the block was created
        /// </summary>
        /// <remarks>
        /// It's used to verify validity of the block
        /// </remarks>
        /// <example>
        /// 0001000000000000000000000000000000000000000000000000000000000000
        /// </example>
        public string Difficulty { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor to deserialize with NewtonSoft
        /// </summary>
        public Block() { }

        /// <summary>
        /// Constructor to create a block already in the chain
        /// </summary>
        /// <param name="index"></param>
        /// <param name="owner"></param>
        /// <param name="creationDate"></param>
        /// <param name="previousHash"></param>
        /// <param name="transfers"></param>
        /// <param name="difficulty"></param>
        /// <remarks>
        /// It's used only for test purpose
        /// This constructor :
        /// - Set the Guid to a new one
        /// - Calculate the Hash of the block
        /// </remarks>
        public Block(int index, string owner, DateTime creationDate, string previousHash, IEnumerable<Transfer> transfers, string difficulty)
        {
            Index = index;
            Owner = owner;
            CreationDate = creationDate;
            PreviousHash = previousHash;
            Transfers = transfers.ToArray();
            Difficulty = difficulty;
            Guid = Guid.NewGuid();
            Hash = CalculateHash();
        }

        /// <summary>
        /// Constructor to initialize a new block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="owner"></param>
        /// <param name="previousHash"></param>
        /// <param name="transfers"></param>
        /// <param name="difficulty"></param>
        /// <remarks>
        /// This constructor :
        /// - Set the Guid to a new one
        /// - Calculate the Hash of the block
        /// </remarks>
        public Block(int index, string owner, string previousHash, IEnumerable<Transfer> transfers, string difficulty)
        {
            Index = index;
            Owner = owner;
            CreationDate = DateTime.UtcNow;
            PreviousHash = previousHash;
            Transfers = transfers.ToArray();
            Difficulty = difficulty;
            Guid = Guid.NewGuid();
            Hash = CalculateHash();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculate the Hash from creationDate, previousHash, transfers and Guid
        /// </summary>
        /// <returns>
        /// The calculated hash with a SHA256, so it will be 256 characters long
        /// </returns>
        public string CalculateHash()
        {
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes($"{CreationDate}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transfers)}-{Guid}"))
                    .Select(item => item.ToString("x2"))).ToUpper();
            }
        }

        /// <summary>
        /// Try if the Hash is correct for the given difficulty
        /// </summary>
        /// <param name="difficulty">
        /// The difficulty is the high limit for hash in hexadecimal
        /// That's why is begin with some 0 most of the time
        /// More 0 means more difficulty to have a working hash
        /// </param>
        /// <returns>
        /// If the Hash is below the Difficulty
        /// </returns>
        public bool TryHash(string difficulty)
        {
            PreviousHash = MainViewModel.BlockChain.LastBlock.Hash;
            Transfers = MainViewModel.BlockChain.PendingTransfers.ToArray();

            Guid = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
            Hash = CalculateHash();

            return Hash.IsLowerHex(difficulty);
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Compare 2 blocks with :
        /// - Index
        /// - CreationDate
        /// - Hash
        /// - PreviousHash
        /// - Transaction list
        /// - Guid
        /// </summary>
        /// <param name="obj">
        /// The compared object, transform in Block object
        /// </param>
        public override bool Equals(object obj)
        {
            if (!(obj is Block other))
                return false;

            return Index == other.Index &&
                   CreationDate.Equals(other.CreationDate) &&
                   string.Equals(PreviousHash, other.PreviousHash) &&
                   Transfers.SequenceEqual(other.Transfers) &&
                   Guid.Equals(other.Guid) &&
                   string.Equals(Hash, other.Hash);
        }

        /// <summary>
        /// The ToString() method
        /// </summary>
        /// <returns>
        /// Concatenate Index, CreationDate, Transaction list and sum of the amount and biscuit
        /// </returns>
        /// <example>
        /// 42 (03/06/2010 13:52:45) | 14 transfers | 10 coins
        /// </example>
        public override string ToString()
        {
            return $"{Index} ({CreationDate}) | {Transfers.Count()} transfers | {Transfers.Sum(t => t.Amount + t.Biscuit)} coins";
        }

        #endregion
    }
}
