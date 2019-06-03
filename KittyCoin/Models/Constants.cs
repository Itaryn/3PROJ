namespace KittyCoin.Models
{
    /// <summary>
    /// Contain all the constant used in project KittyCoin
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The private word who is used to send money to the miner
        /// </summary>
        public static string PRIVATE_WORDS_KITTYCHAIN => "ThereAreTwoMeansOfRefugeFromTheMiseryOfLifeMusicAndCats";

        /// <summary>
        /// The name of the file used to saved the blockchain
        /// </summary>
        public static string SAVE_FILENAME => ".\\Resources\\Blockchain.txt";

        /// <summary>
        /// The interval for 2 block creation expected (in second)
        /// </summary>
        public static int BLOCK_CREATION_TIME_EXPECTED => 10 * 1;

        /// <summary>
        /// The interval into 2 difficulty check
        /// </summary>
        public static int NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY => 16;

        /// <summary>
        /// The interval between 2 save (in second)
        /// </summary>
        public static int SCHEDULE_SAVE_TIME => 5 * 60;

        /// <summary>
        /// The beginning of the server address
        /// </summary>
        public static string SERVER_ADDRESS => "ws://";

        /// <summary>
        /// The name of the web service, used in the server address
        /// </summary>
        public static string WEB_SERVICE_NAME => $"/{BLOCKCHAIN}";

        /// <summary>
        /// The Blockchain word
        /// </summary>
        public static string BLOCKCHAIN => "Blockchain";

        /// <summary>
        /// The Transfer word
        /// </summary>
        public static string TRANSFER => "Transfer";

        /// <summary>
        /// The Transfers word
        /// </summary>
        public static string TRANSFERS => "Transfers";

        /// <summary>
        /// The Block word
        /// </summary>
        public static string BLOCK => "Block";

        /// <summary>
        /// The Blocks word
        /// </summary>
        public static string BLOCKS => "Blocks";

        /// <summary>
        /// The Blockchain is not valid sentence
        /// </summary>
        public static string BLOCKCHAIN_IS_NOT_VALID => "Blockchain is not valid";

        /// <summary>
        /// The Blockchain miss blocks sentence
        /// </summary>
        public static string BLOCKCHAIN_MISS_BLOCK => "Blockchain miss blocks";

        /// <summary>
        /// The Blockchain overwrite word
        /// </summary>
        public static string BLOCKCHAIN_OVERWRITE => "BlockChainOverwrite";

        /// <summary>
        /// The Need blockchain word
        /// </summary>
        public static string NEED_BLOCKCHAIN => "NeedBlockchain";

        /// <summary>
        /// The max value for waiting the blockchain object (in second)
        /// </summary>
        public static int WAITING_TIME_MAX => 10;

        /// <summary>
        /// The Get server word
        /// </summary>
        public static string GET_SERVERS => "GetServers";

        /// <summary>
        /// The Wallet connected message
        /// </summary>
        public static string WALLET_CONNECTED => "Your wallet is connected";
    }
}
