using KittyCoins.ViewModels;
using System.Windows.Controls;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class NewTransactionView : UserControl
    {
        public NewTransactionViewModel ViewModel;

        public NewTransactionView()
        {
            InitializeComponent();
            ViewModel = new NewTransactionViewModel();
            DataContext = ViewModel;
        }
    }
}
