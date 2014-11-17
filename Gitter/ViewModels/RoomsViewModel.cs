using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Akavache;
using Gitter.Models;
using ReactiveUI;
using Splat;

namespace Gitter.ViewModels
{
    public class RoomsViewModel : ReactiveObject, IRoutableViewModel
    {
        private RoomViewModel selectedRoom;

        public RoomsViewModel(IGitterApi api = null, IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Rooms = new ReactiveList<RoomViewModel>();

            this.LoadRooms = ReactiveCommand.CreateAsyncObservable(_ => LoadRoomsImpl(api ?? GitterApi.UserInitiated));
            this.LoadRooms.Subscribe(x =>
            {
                using (this.Rooms.SuppressChangeNotifications())
                {
                    this.Rooms.Clear();
                    this.Rooms.AddRange(x);
                }
            });

            this.WhenAnyValue(x => x.SelectedRoom)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    this.SelectedRoom = null;
                    this.HostScreen.Router.Navigate.Execute(new MessagesViewModel(x.Room));
                });
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<IEnumerable<RoomViewModel>> LoadRooms { get; private set; }

        public IReactiveList<RoomViewModel> Rooms { get; private set; }

        public RoomViewModel SelectedRoom
        {
            get { return this.selectedRoom; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRoom, value); }
        }

        public string UrlPathSegment
        {
            get { return "Rooms"; }
        }

        private static IObservable<IEnumerable<RoomViewModel>> LoadRoomsImpl(IGitterApi api)
        {
            return BlobCache.LocalMachine.GetAndFetchLatest("rooms", () =>
                GitterApi.GetAccessToken()
                .SelectMany(api.GetRooms))
                .DistinctUntilChanged(new RoomSetEqualityComparer()) // We don't want the rooms to refresh the UI twice when they haven't changed
                .Select(rooms => rooms.OrderBy(room => room.name, StringComparer.CurrentCulture).Select(room => new RoomViewModel(room)));
        }

        private class RoomSetEqualityComparer : IEqualityComparer<IEnumerable<Room>>
        {
            public bool Equals(IEnumerable<Room> x, IEnumerable<Room> y)
            {
                return new HashSet<Room>(x).SetEquals(y);
            }

            public int GetHashCode(IEnumerable<Room> obj)
            {
                unchecked
                {
                    int hashCode = 1;

                    foreach (Room room in obj)
                    {
                        hashCode *= room.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }
    }
}