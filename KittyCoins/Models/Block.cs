using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainDemo
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime CreationDate { get; set; }
        public string PreviousHash { get; set; }
        public IList<Transfer> Transfers { get; set; }
        public Guid Guid { get; set; }
        public string Hash { get; set; }

        public Block(DateTime creationDate, string previousHash, IList<Transfer> transfers, int index = 0)
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

        public void Mine(int difficulty)
        {
            var firstK = new string('K', difficulty);
            while (Hash == null || Hash.Substring(0, difficulty) != firstK)
            {
                Guid = new Guid();
                Hash = CalculateHash();
            }
        }
    }
}
