using System.Threading;
using System.Windows;
using KittyCoin.Views;

namespace KittyCoin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var splashScreen = new Views.SplashScreen();
            var mw = new MainView();
            splashScreen.Show();

            Thread.Sleep(2000);
            splashScreen.Close();

            mw.Show();
        }
    }
}
