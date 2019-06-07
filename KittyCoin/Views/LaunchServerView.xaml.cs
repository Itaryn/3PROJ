using System.Windows.Input;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for LaunchServerView.xaml
    /// </summary>
    public partial class LaunchServerView
    {
        public LaunchServerViewModel ViewModel;

        public LaunchServerView()
        {
            InitializeComponent();
            ViewModel = new LaunchServerViewModel();
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
