namespace KittyCoins.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    public class Block
    {
        public int Index { get; set; }
        public DateTime CreationDate { get; set; }
        public string PreviousHash { get; set; }
        public List<Transfer> Transfers { get; set; }
        public Guid Guid { get; set; }
        public string Hash { get; set; }

        public Block() { }

        public Block(int index, DateTime creationDate, string previousHash, IEnumerable<Transfer> transfers)
        {
            Index = index;
            CreationDate = creationDate;
            PreviousHash = previousHash;
            Transfers = transfers.ToList();
            Guid = new Guid();
            Hash = CalculateHash();
        }
        public Block(int index, string previousHash, IEnumerable<Transfer> transfers)
        {
            Index = index;
            CreationDate = DateTime.UtcNow;
            PreviousHash = previousHash;
            Transfers = transfers.ToList();
            Guid = new Guid();
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes($"{CreationDate}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transfers)}-{Guid}"))
                    .Select(item => item.ToString("x2")));
            }
        }

        public bool TryHash(int difficulty)
        {
            var firstK = new string('K', difficulty);
            Guid = new Guid();
            Hash = CalculateHash();
            return Hash.StartsWith(firstK);
        }

        protected bool Equals(Block other)
        {
            return Index == other.Index &&
                   CreationDate.Equals(other.CreationDate) &&
                   string.Equals(PreviousHash, other.PreviousHash) &&
                   Transfers.SequenceEqual(other.Transfers) &&
                   Guid.Equals(other.Guid) &&
                   string.Equals(Hash, other.Hash);
        }

        public override string ToString()
        {
            return $"{Index} ({CreationDate}) | {Transfers.Count} transfers";
        }
    }
}
