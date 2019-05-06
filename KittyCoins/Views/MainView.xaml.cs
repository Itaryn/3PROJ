namespace KittyCoins.Views
{
    using KittyCoins.Models;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private MainViewModel _viewModel;
        private Thread ScheduleTask;
        public MainView()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            ScheduleTask = new Thread(SaveBlockchain);
            ScheduleTask.Start();

            if (!Directory.Exists(Constants.DATABASE_FOLDER))
                Directory.CreateDirectory(Constants.DATABASE_FOLDER);

            var cspParams = new CspParameters {KeyContainerName = "Salut ezgziohvsuowghvuqz"};

            // Specify the container name using the passed variable.
            var RSA = new RSACryptoServiceProvider(cspParams);

            // Specify the container name using the passed variable.
            var RSA2 = new RSACryptoServiceProvider(cspParams);

            Console.WriteLine(RSA.ToXmlString(true));
            Console.WriteLine(RSA2.ToXmlString(true));
            Console.WriteLine(RSA.ToXmlString(true).Equals(RSA2.ToXmlString(true)));
            Console.WriteLine(RSA.ToXmlString(false).Equals(RSA2.ToXmlString(false)));

            var privateKey = RSA.ExportParameters(true);

            var publicKey = RSA.ExportParameters(false);

            //Tried with and without the whole base64 thing
            //var messageToSign = new Transfer(Convert.ToBase64String(publicKey.Modulus), "zergzergzqgerg", 10, 2, privateKey);
            
            //// Is this message really, really, REALLY sent by me?
            //var success = messageToSign.VerifyData();

            //Console.WriteLine("Is this message really, really, REALLY sent by me? " + success);
        }

        private void ScrollToTheEnd(object sender, TextChangedEventArgs e)
        {
            ConsoleGUI.ScrollToEnd();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Close all thread if they are on
            _viewModel.MiningThread?.Abort();
            MainViewModel.Client.Close();
            _viewModel.Server?.wss.Stop();
            ScheduleTask.Abort();

            Application.Current.Shutdown();
            base.OnClosing(e);
        }

        private void SaveBlockchain()
        {
            while (true)
            {
                Thread.Sleep(5 * 60 * 1000);
                foreach (var block in MainViewModel.BlockChain.Chain)
                {
                    var pathFile = Path.Combine(Constants.DATABASE_FOLDER, $"{Constants.BLOCK_FILENAME}{block.Index.ToString()}{Constants.BLOCK_FILE_EXTENSION}");
                    var json = JsonConvert.SerializeObject(block, Formatting.Indented);

                    if (File.Exists(pathFile) && File.ReadAllText(pathFile) == json)
                        continue;

                    File.WriteAllText(pathFile, json);
                    _viewModel.Console = $"Block n°{block.Index} saved in a file";
                }
            }
        }
    }
}
