using System;
using System.Reactive.Linq;
using Gitter.ViewModels;
using ReactiveUI;
using Xamarin.Forms;

namespace Gitter.Views
{
    public partial class MessagesPage : MasterDetailPage, IViewFor<MessagesViewModel>
    {
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create<MessagesPage, MessagesViewModel>(x => x.ViewModel, default(MessagesViewModel));

        public MessagesPage()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(x => this.BindingContext = x);

            this.WhenAnyValue(x => x.ViewModel)
               .Where(x => x != null)
               .InvokeCommand(this, x => x.ViewModel.LoadMessages);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .InvokeCommand(this, x => x.ViewModel.LoadUsers);

            this.Bind(this.ViewModel, x => x.MessageText, x => x.MessageTextEntry.Text);
            this.MessageTextEntry.Events().Completed
                .Where(_ => this.ViewModel.SendMessage.CanExecute(null))
                .SelectMany(_ => this.ViewModel.SendMessage.ExecuteAsync()).Subscribe();

            // We don't want messages to be selectable
            this.MessagesListView.Events().ItemSelected
                .Where(x => x != null)
                .Subscribe(_ => this.MessagesListView.SelectedItem = null);
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (MessagesViewModel)value; }
        }

        public MessagesViewModel ViewModel
        {
            get { return (MessagesViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}