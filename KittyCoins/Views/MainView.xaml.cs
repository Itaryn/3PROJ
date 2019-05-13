namespace KittyCoins.Views
{
    using System;
    using System.ComponentModel;
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
