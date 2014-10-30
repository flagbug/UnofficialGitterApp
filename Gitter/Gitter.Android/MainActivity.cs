using Android.App;
using Android.Content.PM;
using Android.OS;

using Xamarin.Forms.Platform.Android;

namespace Gitter.Droid
{
    [Activity(Label = "Gitter", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : AndroidActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);

            SetPage(App.GetMainPage());
        }
    }
}