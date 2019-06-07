using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for ShowBlockchainView.xaml
    /// </summary>
    public partial class ShowBlockchainView
    {

        public ShowBlockchainView()
        {
            InitializeComponent();
            DataContext = new ShowBlockchainViewModel();
        }
    }
}
