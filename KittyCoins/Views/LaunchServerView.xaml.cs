using KittyCoins.ViewModels;
using System.Windows.Controls;

namespace KittyCoins.Views
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
    }
}
