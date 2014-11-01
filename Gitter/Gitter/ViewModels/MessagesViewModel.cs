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

        public MessagesViewModel(string roomId, IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Messages = new ReactiveList<MessageViewModel>();

            IObservable<string> accessToken = BlobCache.Secure.GetLoginAsync("Gitter").Select(x => x.Password).PublishLast().RefCount();

            IConnectableObservable<MessageViewModel> messageStream = Observable.Defer(() => accessToken)
                .SelectMany(x => this.LoadMessages(roomId, x).SelectMany(y => y.ToObservable()).Concat(this.StreamMessages(roomId, x)))
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
                await this.SendMessageImpl(roomId, this.MessageText, await accessToken);
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

        public string UrlPathSegment
        {
            get { return "Messages"; }
        }

        private IObservable<IReadOnlyList<Message>> LoadMessages(string roomId, string accessToken)
        {
            var client = new HttpClient(NetCache.UserInitiated)
            {
                BaseAddress = new Uri("https://api.gitter.im/v1")
            };

            var api = RestService.For<IGitterApi>(client);

            return api.GetMessages(roomId, "Bearer " + accessToken).Finally(() => client.Dispose());
        }

        private Task SendMessageImpl(string roomId, string text, string accessToken)
        {
            var client = new HttpClient(NetCache.UserInitiated)
            {
                BaseAddress = new Uri("https://api.gitter.im/v1")
            };

            var api = RestService.For<IGitterApi>(client);

            return api.SendMessage(roomId, new SendMessage(text), "Bearer " + accessToken);
        }

        private IObservable<Message> StreamMessages(string roomId, string accessToken)
        {
            var streamApi = new GitterStreamingApi();

            return BlobCache.Secure.GetLoginAsync("Gitter")
                .SelectMany(x => streamApi.ObserveMessages(roomId, accessToken));
        }
    }
}