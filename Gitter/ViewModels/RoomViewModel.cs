using System;
using System.Reactive.Linq;
using Gitter.Models;
using ReactiveUI;

namespace Gitter.ViewModels
{
    public class RoomViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<MessagesViewModel> messages;

        public RoomViewModel(Room room)
        {
            if (room == null)
                throw new ArgumentNullException("room");

            this.Room = room;

            this.LoadMessages = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                var vm = new MessagesViewModel(this.Room);
                return vm.LoadMessages.ExecuteAsync().Select(__ => vm);
            });

            this.messages = this.LoadMessages.ToProperty(this, x => x.Messages);
        }

        public string Id
        {
            get { return this.Room.id; }
        }

        public IReactiveCommand<MessagesViewModel> LoadMessages { get; private set; }

        public MessagesViewModel Messages
        {
            get { return this.messages.Value; }
        }

        public string Name
        {
            get { return this.Room.name; }
        }

        public Room Room { get; private set; }

        public string Topic
        {
            get { return this.Room.topic; }
        }

        public Uri UserAvatarSource
        {
            get
            {
                if (this.Room.user != null && this.Room.user.avatarUrlSmall != null)
                {
                    return new Uri(this.Room.user.avatarUrlSmall);
                }

                return null;
            }
        }
    }
}