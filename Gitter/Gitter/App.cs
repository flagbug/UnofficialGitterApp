using Xamarin.Forms;

namespace Gitter
{
    public class App
    {
        public static Page GetMainPage()
        {
            return new ContentPage
            {
                Content = new Label
                {
                    Text = "Hello, Forms !",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                },
            };
        }
    }
}