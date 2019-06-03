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
        public int Index { get; set; }

        /// <summary>
        /// Creator of the block
        /// </summary>
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
        /// List of transfers contained in the block
        /// </summary>
        public Transfer[] Transfers { get; set; }

        /// <summary>
        /// A random Guid to generate the hash beginning with K
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The Hash of the block
        /// </summary>
        public string Hash { get; set; }

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
        /// <param name="creationDate"></param>
        /// <param name="previousHash"></param>
        /// <param name="transfers"></param>
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
        /// <param name="previousHash"></param>
        /// <param name="transfers"></param>
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
        /// The calculated hash
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
        /// Try if the Hash is correct for the actual difficulty
        /// </summary>
        /// <param name="difficulty">
        /// Numbers of K needed
        /// </param>
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
        /// Compare 2 blocks
        /// </summary>
        /// <param name="obj">The compared block</param>
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
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Index} ({CreationDate}) | {Transfers.Count()} transfers | {Transfers.Sum(t => t.Amount + t.Biscuit)} coins";
        }

        #endregion
    }
}
