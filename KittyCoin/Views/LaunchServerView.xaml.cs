using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for LaunchServerView.xaml
    /// </summary>
    public partial class LaunchServerView : UserControl
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
