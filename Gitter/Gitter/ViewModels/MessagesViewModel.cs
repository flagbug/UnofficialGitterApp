using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Gitter.ViewModels
{
    public class MessagesViewModel : ReactiveObject, IRoutableViewModel
    {
        public MessagesViewModel(IScreen hostScreen)
        {
            this.HostScreen = hostScreen;
        }

        public IScreen HostScreen { get; private set; }

        public string UrlPathSegment
        {
            get { return "Messages"; }
        }
    }
}