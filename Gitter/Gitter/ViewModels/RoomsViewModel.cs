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
        public RoomsViewModel(IScreen hostScreen = null)
        {
            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

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
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<IEnumerable<RoomViewModel>> LoadRooms { get; private set; }

        public IReactiveList<RoomViewModel> Rooms { get; private set; }

        public string UrlPathSegment
        {
            get { return "Rooms"; }
        }

        private async Task<IEnumerable<RoomViewModel>> LoadRoomsImpl()
        {
            var client = new HttpClient(NetCache.UserInitiated)
            {
                BaseAddress = new Uri("https://api.gitter.im/v1"),
            };

            var api = RestService.For<IGitterApi>(client);

            LoginInfo loginInfo = await BlobCache.Secure.GetLoginAsync("Gitter");

            IReadOnlyList<Room> rooms = await api.GetRooms("Bearer " + loginInfo.Password);

            return rooms.Select(room => new RoomViewModel(room));
        }
    }
}