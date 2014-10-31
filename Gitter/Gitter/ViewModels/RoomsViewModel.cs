using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Fusillade;
using Gitter.Models;
using ReactiveUI;
using Refit;
using Splat;

namespace Gitter.ViewModels
{
    public class RoomsViewModel : ReactiveObject, IRoutableViewModel
    {
        private RoomViewModel selectedRoom;

        public RoomsViewModel(IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Rooms = new ReactiveList<RoomViewModel>();

            this.LoadRooms = ReactiveCommand.CreateAsyncTask(_ => this.LoadRoomsImpl());
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
                .Subscribe(x => this.HostScreen.Router.Navigate.Execute(new MessagesViewModel(x.Id)));
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

        private async Task<IEnumerable<RoomViewModel>> LoadRoomsImpl()
        {
            using (var client = new HttpClient(NetCache.UserInitiated))
            {
                client.BaseAddress = new Uri("https://api.gitter.im/v1");

                var api = RestService.For<IGitterApi>(client);

                LoginInfo loginInfo = await BlobCache.Secure.GetLoginAsync("Gitter");

                IReadOnlyList<Room> rooms = await api.GetRooms("Bearer " + loginInfo.Password);

                return rooms.Select(room => new RoomViewModel(room));
            }
        }
    }
}