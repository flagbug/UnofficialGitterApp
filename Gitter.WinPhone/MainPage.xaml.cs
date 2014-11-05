using Microsoft.Phone.Controls;
using ReactiveUI;
using Xamarin.Forms;

namespace Gitter.WinPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            Forms.Init();

            var bootstrapper = RxApp.SuspensionHost.GetAppState<AppBootstrapper>();
            bootstrapper.CreateMainPage().ConvertPageToUIElement(this);
        }
    }
}