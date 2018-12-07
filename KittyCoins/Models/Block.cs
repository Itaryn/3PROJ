using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace KittyCoins.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime CreationDate { get; set; }
        public string PreviousHash { get; set; }
        public IList<Transfer> Transfers { get; set; }
        public Guid Guid { get; set; }
        public string Hash { get; set; }

        public Block(int index, DateTime creationDate, string previousHash, IList<Transfer> transfers)
        {
            Index = index;
            CreationDate = creationDate;
            PreviousHash = previousHash;
            Transfers = transfers;
        }

        public string CalculateHash()
        {
            var sha256 = SHA256.Create();

            var inputBytes = Encoding.ASCII.GetBytes($"{CreationDate}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transfers)}-{Guid}");
            var outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        public bool TryHash(int difficulty)
        {
            var firstK = new string('K', difficulty);
            Guid = new Guid();
            Hash = CalculateHash();
            return Hash.StartsWith(firstK);
        }

        public override bool Equals(object obj)
        {
            Block compareBlock;
            try { compareBlock = (Block)obj; }
            catch (Exception) { return false; }
            if (compareBlock == null) return false;

            return Index.Equals(compareBlock.Index) &&
                   CreationDate.Equals(compareBlock.CreationDate) &&
                   PreviousHash.Equals(compareBlock.PreviousHash) &&
                   Transfers.Equals(compareBlock.Transfers) &&
                   Guid.Equals(compareBlock.Guid) &&
                   Hash.Equals(compareBlock.Hash);
        }
    }
}
