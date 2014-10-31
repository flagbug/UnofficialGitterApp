using System;
using Gitter.Models;

namespace Gitter.ViewModels
{
    public class RoomViewModel
    {
        private readonly Room room;

        public RoomViewModel(Room room)
        {
            if (room == null)
                throw new ArgumentNullException("room");

            this.room = room;
        }

        public string Id
        {
            get { return this.room.id; }
        }

        public string Name
        {
            get { return this.room.name; }
        }
    }
}