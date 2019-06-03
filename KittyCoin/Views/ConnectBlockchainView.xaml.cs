using System.Collections.Generic;
using System.Windows.Controls;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ConnectBlockchainView : UserControl
    {
        public ConnectBlockchainViewModel ViewModel;

        public ConnectBlockchainView(IEnumerable<string> serverList)
        {
            InitializeComponent();
            ViewModel = new ConnectBlockchainViewModel(serverList);
            DataContext = ViewModel;
        }
    }
}
