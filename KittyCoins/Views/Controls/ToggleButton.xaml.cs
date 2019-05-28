using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KittyCoins.Views.Controls
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        Thickness LeftSide = new Thickness(-45, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -45, 0);
        SolidColorBrush BackOn = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush BackOff = new SolidColorBrush(Color.FromRgb(0, 78, 139));
        SolidColorBrush DotOn = new SolidColorBrush(Color.FromRgb(0, 121, 216));
        SolidColorBrush DotOff = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private bool _toggled;

        public ToggleButton()
        {
            InitializeComponent();
            SetToggleState(false);
        }

        public bool Toggled { get => _toggled; set => _toggled = value; }

        private void ToggleClick(object sender, MouseButtonEventArgs e)
        {
            SetToggleState(!_toggled);
        }

        public void SetToggleState(bool state)
        {
            if (state)
            {
                Back.Fill = BackOn;
                Dot.Fill = DotOn;
                Dot.Margin = RightSide;
                _toggled = true;
            }
            else
            {
                Back.Fill = BackOff;
                Dot.Fill = DotOff;
                Dot.Margin = LeftSide;
                _toggled = false;
            }
        }
    }
}
