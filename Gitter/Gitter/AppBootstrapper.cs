using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

namespace Gitter
{
    public class AppBootstrapper : ReactiveObject, IScreen
    {
        public AppBootstrapper()
        {
            Router = new RoutingState();
            Locator.CurrentMutable.RegisterConstant(this, typeof(IScreen));

            // TODO: Register new views here, then navigate to the first page in your app
            // Locator.CurrentMutable.Register(() => new TestView(), typeof(IViewFor<TestViewModel>));

            //Router.Navigate.Execute(new TestViewModel(this));
        }

        public RoutingState Router { get; protected set; }

        public Page CreateMainPage()
        {
            return new RoutedViewHost();
        }
    }
}