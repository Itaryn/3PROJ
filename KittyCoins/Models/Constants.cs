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
        public static string DATABASE_NAME => "Database.db3";
        public static string DATABASE_FOLDER => ".\\Resources";
        public static int BLOCK_CREATION_TIME_EXPECTED => 10 * 1;
        public static int NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY => 16;
    }
}
