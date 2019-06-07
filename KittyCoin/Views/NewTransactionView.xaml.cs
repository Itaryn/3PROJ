using System.Windows.Controls;
using System.Windows.Input;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for NewTransactionView.xaml
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

        private void ButtonOnMouseEnter(object sender, MouseEventArgs e)
        {
            CodeBehindCommon.ButtonChangeCat(sender, true);
        }

        private void ButtonOnMouseLeave(object sender, MouseEventArgs e)
        {
            CodeBehindCommon.ButtonChangeCat(sender, false);
        }
    }
}
