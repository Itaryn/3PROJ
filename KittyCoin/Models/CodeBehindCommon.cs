using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KittyCoin.Models
{
    public class CodeBehindCommon
    {
        public static void ButtonChangeCat(object sender, bool enter)
        {
            if (sender is Button but &&
                but.Content is Grid grid)
            {
                var images = grid.Children.OfType<Image>();
                foreach (var image in images)
                {
                    if (enter)
                    {
                        image.Source = new BitmapImage(new Uri(@"../Resources/Image/icons8-chat-100-up.png", UriKind.Relative));
                    }
                    else
                    {
                        image.Source = new BitmapImage(new Uri(@"../Resources/Image/icons8-chat-100-down.png", UriKind.Relative));
                    }
                }
            }
        }
    }
}
