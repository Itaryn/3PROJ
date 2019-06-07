using System.Windows;
using System.Windows.Input;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView
    {
        public RegisterView()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
        }

        private void ButtonOnMouseEnter(object sender, MouseEventArgs e)
        {
            CodeBehindCommon.ButtonChangeCat(sender, true);
        }

        private void ButtonOnMouseLeave(object sender, MouseEventArgs e)
        {
            CodeBehindCommon.ButtonChangeCat(sender, false);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
