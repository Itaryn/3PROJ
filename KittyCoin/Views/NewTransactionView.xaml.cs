using System.Windows.Controls;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
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
