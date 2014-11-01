using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using Fusillade;
using Gitter.Models;
using ReactiveUI;
using Refit;
using Splat;

namespace Gitter.ViewModels
{
    public class MessagesViewModel : ReactiveObject, IRoutableViewModel
    {
        private string messageText;

        public MessagesViewModel(Room room, IGitterApi api = null, IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Messages = new ReactiveList<MessageViewModel>();
            this.UrlPathSegment = room.name;

            IObservable<string> accessToken = GitterApi.GetAccessToken().PublishLast().RefCount();

            IConnectableObservable<MessageViewModel> messageStream = accessToken
                .SelectMany(token =>
                    // Fetch the messages every 10 seconds or when we've sent a message This is a
                    // workaround till the message streaming works
                    this.SendMessage.StartWith(Unit.Default)
                    .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(10), RxApp.TaskpoolScheduler))
                    .Select(_ => Unit.Default)
                    .StartWith(Unit.Default)
                    .SelectMany(_ => (api ?? GitterApi.UserInitiated).GetMessages(room.id, token).Do(__ => this.Messages.Clear()).SelectMany(y => y.ToObservable()))
                /*.Concat(this.StreamMessages(roomId, x))*/) // Something is trolling us, message streaming isn't working currently
                .Select(x => new MessageViewModel(x))
                .Publish();

            messageStream.Subscribe(x => this.Messages.Add(x));

            this.LoadMessageStream = ReactiveCommand.CreateAsyncTask(_ =>
            {
                Task firstMessage = messageStream.FirstAsync().ToTask();

                messageStream.Connect();

                return firstMessage;
            });

            this.SendMessage = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                await (api ?? GitterApi.UserInitiated).SendMessage(room.id, new SendMessage(this.MessageText), await accessToken);
                this.MessageText = String.Empty;
            });
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<Unit> LoadMessageStream { get; private set; }

        public IReactiveList<MessageViewModel> Messages { get; private set; }

        public string MessageText
        {
            get { return this.messageText; }
            set { this.RaiseAndSetIfChanged(ref this.messageText, value); }
        }

        public ReactiveCommand<Unit> SendMessage { get; private set; }

        public string UrlPathSegment { get; private set; }

        private IObservable<Message> StreamMessages(string roomId, string accessToken)
        {
            var streamApi = new GitterStreamingApi();

            return BlobCache.Secure.GetLoginAsync("Gitter")
                .SelectMany(x => streamApi.ObserveMessages(roomId, accessToken));
        }
    }
}