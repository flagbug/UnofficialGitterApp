using System;
using Gitter.Models;
using Xamarin.Forms;

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

        public ImageSource ImageSource
        {
            get
            {
                if (this.message.fromUser != null && this.message.fromUser.avatarUrlSmall != null)
                {
                    return new UriImageSource { Uri = new Uri(this.message.fromUser.avatarUrlSmall) };
                }

                return null;
            }
        }

        public string Sender
        {
            get
            {
                if (this.message.fromUser != null)
                {
                    return this.message.fromUser.username;
                }

                return null;
            }
        }

        public string Text
        {
            get { return this.message.text; }
        }
    }
}