using System;
using Gitter.Models;

namespace Gitter.ViewModels
{
    public class MessageViewModel
    {
        private readonly Message message;

        public MessageViewModel(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            this.message = message;
        }

        public string Text
        {
            get { return this.message.text; }
        }
    }
}