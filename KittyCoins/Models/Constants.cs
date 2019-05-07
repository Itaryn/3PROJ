using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KittyCoins.Models
{
    public static class Constants
    {
        public static string PRIVATE_WORDS_KITTYCHAIN => "ThereAreTwoMeansOfRefugeFromTheMiseryOfLifeMusicAndCats";
        public static string DATABASE_FOLDER => ".\\Resources\\Blocks";
        public static int BLOCK_CREATION_TIME_EXPECTED => 10 * 1;
        public static int NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY => 16;
        public static string SERVER_ADDRESS => "ws://";
        public static string WEB_SERVICE_NAME => $"/{BLOCKCHAIN}";
        public static string BLOCKCHAIN => "Blockchain";
        public static string TRANSFER => "Transfer";
        public static string TRANSFERS => "Transfers";
        public static string BLOCK => "Block";
        public static string BLOCKS => "Blocks";
        public static string BLOCKCHAIN_IS_NOT_VALID => "Blockchain is not valid";
        public static string BLOCKCHAIN_MISS_BLOCK => "Blockchain miss blocks";
        public static string BLOCKCHAIN_OVERWRITE => "BlockChainOverwrite";
        public static string NEED_BLOCKCHAIN => "NeedBlockchain";
    }
}
