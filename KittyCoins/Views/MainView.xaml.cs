namespace KittyCoins.Views
{
    using System;
    using System.Security.Cryptography;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private MainViewModel _viewModel;
        public MainView()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
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
            _viewModel.MiningThread?.Abort();
            _viewModel.Client?.Close();
            _viewModel.Server?.wss.Stop();

            Application.Current.Shutdown();
            base.OnClosing(e);
        }
    }
}
