using System.Windows;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for BlockChainView.xaml
    /// </summary>
    public partial class RegisterView : Window
    {
        private RegisterViewModel _viewModel;
        public RegisterView()
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel();
            DataContext = _viewModel;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
