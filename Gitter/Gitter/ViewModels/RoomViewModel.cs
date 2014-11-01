using System;
using Gitter.Models;

namespace Gitter.ViewModels
{
    public class RoomViewModel
    {
        public RoomViewModel(Room room)
        {
            if (room == null)
                throw new ArgumentNullException("room");

            this.Room = room;
        }

        public string Id
        {
            get { return this.Room.id; }
        }

        public string Name
        {
            get { return this.Room.name; }
        }

        public Room Room { get; private set; }
    }
}