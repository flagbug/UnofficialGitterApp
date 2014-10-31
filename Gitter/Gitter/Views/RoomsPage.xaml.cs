using System.Reactive.Linq;
using Gitter.ViewModels;
using ReactiveUI;
using Xamarin.Forms;

namespace Gitter.Views
{
    public partial class RoomsPage : ContentPage, IViewFor<RoomsViewModel>
    {
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create<RoomsPage, RoomsViewModel>(x => x.ViewModel, default(RoomsViewModel));

        public RoomsPage()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel)
               .Where(x => x != null)
               .InvokeCommand(this, x => x.ViewModel.LoadRooms);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (RoomsViewModel)value; }
        }

        public RoomsViewModel ViewModel
        {
            get { return (RoomsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}