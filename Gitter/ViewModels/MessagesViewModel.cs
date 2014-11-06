using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using Gitter.Models;
using ReactiveUI;
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

            this.LoadMessages = ReactiveCommand.CreateAsyncObservable(_ => accessToken.SelectMany(token => (api ?? GitterApi.UserInitiated).GetMessages(room.id, token)));

            this.SendMessage = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.MessageText, x => !String.IsNullOrWhiteSpace(x)), async _ =>
            {
                await (api ?? GitterApi.UserInitiated).SendMessage(room.id, new SendMessage(this.MessageText), await accessToken);
                this.MessageText = String.Empty;
            });

            this.LoadMessages.FirstAsync()
                // Fetch the messages every 10 seconds or when we've sent a message
                //
                // This is a workaround till the message streaming works
                .Concat(this.SendMessage.StartWith(Unit.Default).SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(10), RxApp.TaskpoolScheduler).Select(__ => Unit.Default)).Merge(this.SendMessage)
                    .SelectMany(__ => this.LoadMessages.ExecuteAsync()))
                .Select(x => x.Select(y => new MessageViewModel(y)))
                .Subscribe(x =>
                {
                    using (this.Messages.SuppressChangeNotifications())
                    {
                        this.Messages.Clear();
                        this.Messages.AddRange(x);
                    }
                });
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<IReadOnlyList<Message>> LoadMessages { get; private set; }

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