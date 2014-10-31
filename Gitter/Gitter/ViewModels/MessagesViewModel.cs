using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using Fusillade;
using ReactiveUI;
using Refit;
using Splat;

namespace Gitter.ViewModels
{
    public class MessagesViewModel : ReactiveObject, IRoutableViewModel
    {
        public MessagesViewModel(string roomId, IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Messages = new ReactiveList<MessageViewModel>();

            IConnectableObservable<MessageViewModel> messageStream = Observable.Defer(() => this.StreamMessages(roomId))
                .Publish();

            messageStream.Subscribe(x => this.Messages.Add(x));

            this.LoadMessageStream = ReactiveCommand.CreateAsyncTask(_ =>
            {
                Task firstMessage = messageStream.FirstAsync().ToTask();

                messageStream.Connect();

                return firstMessage;
            });
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<Unit> LoadMessageStream { get; private set; }

        public IReactiveList<MessageViewModel> Messages { get; private set; }

        public string UrlPathSegment
        {
            get { return "Messages"; }
        }

        private IObservable<MessageViewModel> StreamMessages(string roomId)
        {
            var client = new HttpClient(NetCache.UserInitiated)
            {
                BaseAddress = new Uri("https://api.gitter.im/v1")
            };

            var api = RestService.For<IGitterApi>(client);
            var streamApi = new GitterStreamingApi();

            return BlobCache.Secure.GetLoginAsync("Gitter")
                .SelectMany(x =>
                    api.GetMessages(roomId, "Bearer " + x.Password).Finally(() => client.Dispose()).SelectMany(y => y.ToObservable())
                    .Concat(streamApi.ObserveMessages(roomId, x.Password)))
                .Select(x => new MessageViewModel(x));
        }
    }
}