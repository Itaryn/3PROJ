using KittyCoins.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KittyCoins.Views
{
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

            var launcherServerView = new LaunchServerView();
            launcherServerView.ViewModel.LaunchServerWithPort += _viewModel.LaunchServerMethod;
            MainUserControl.Content = launcherServerView;
        }

        private void MiningClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.ActualUser == null)
            {
                _viewModel.Console = "You must have a connected wallet to be able to mine blocks";

                ButtonMenuClick(new Button { Name = "ConnectWalletButton" }, new RoutedEventArgs());

                MiningToggleButton.SetToggleState(false);
            }

            _viewModel.CheckBoxMine = MiningToggleButton.Toggled;

            MiningTextState.Text = _viewModel.CheckBoxMine ? "On" : "Off";
        }

        private void ButtonMenuClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                LaunchServerButton.Style = FindResource("ButtonMenu") as Style;
                ConnectBlockchainButton.Style = FindResource("ButtonMenu") as Style;
                ConnectWalletButton.Style = FindResource("ButtonMenu") as Style;
                NewTransactionButton.Style = FindResource("ButtonMenu") as Style;
                ShowBlockchainButton.Style = FindResource("ButtonMenu") as Style;
                switch (button.Name)
                {
                    case "LaunchServerButton":
                        LaunchServerButton.Style = FindResource("ButtonMenuClicked") as Style;
                        var launcherServerView = new LaunchServerView();
                        launcherServerView.ViewModel.LaunchServerWithPort += _viewModel.LaunchServerMethod;
                        MainUserControl.Content = launcherServerView;
                        break;
                    case "ConnectBlockchainButton":
                        ConnectBlockchainButton.Style = FindResource("ButtonMenuClicked") as Style;
                        var connectBlockchainView = new ConnectBlockchainView(_viewModel.Client.GetServers());
                        connectBlockchainView.ViewModel.LaunchServerWithPort += _viewModel.ConnectBlockchainMethod;
                        MainUserControl.Content = connectBlockchainView;
                        break;
                    case "ConnectWalletButton":
                        ConnectWalletButton.Style = FindResource("ButtonMenuClicked") as Style;
                        var connectWalletView = new ConnectWalletView(_viewModel.ActualUser?.PublicAddress);
                        connectWalletView.ViewModel.ConnectWithWords += _viewModel.ConnectUserMethod;
                        MainUserControl.Content = connectWalletView;
                        break;
                    case "NewTransactionButton":
                        NewTransactionButton.Style = FindResource("ButtonMenuClicked") as Style;
                        var newTransactionView = new NewTransactionView();
                        newTransactionView.ViewModel.NewTransaction += _viewModel.NewTransactionMethod;
                        MainUserControl.Content = newTransactionView;
                        break;
                    case "ShowBlockchainButton":
                        ShowBlockchainButton.Style = FindResource("ButtonMenuClicked") as Style;
                        var showBlockchainView = new ShowBlockchainView();
                        MainUserControl.Content = showBlockchainView;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ScrollToTheEnd(object sender, TextChangedEventArgs e)
        {
            ConsoleGUI.ScrollToEnd();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            _viewModel.MiningThread?.Abort();
            _viewModel.SaveThread?.Abort();
            _viewModel.Client?.Close();
            _viewModel.Server?.wss.Stop();

            Application.Current.Shutdown();
            base.OnClosing(e);
        }
    }
}
