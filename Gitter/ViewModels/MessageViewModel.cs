using System;
using Gitter.Models;
using Xamarin.Forms;
using Humanizer;

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

        public Uri SenderAvatarSource
        {
            get
            {
                if (this.message.fromUser != null && this.message.fromUser.avatarUrlSmall != null)
                {
                    return new Uri(this.message.fromUser.avatarUrlSmall);
                }

                return null;
            }
        }

        public string Text
        {
            get { return this.message.text; }
        }

        public string Sent
        {
            get
            {
                if (!string.IsNullOrEmpty(this.message.sent))
                {
                    DateTime parsed;
                    if (DateTime.TryParse(this.message.sent, out parsed))
                    {
                        return parsed.Humanize();
                    }
                }
                return string.Empty;
            }
        }
    }
}