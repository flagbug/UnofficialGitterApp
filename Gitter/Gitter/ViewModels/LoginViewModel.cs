using System;
using ReactiveUI;

namespace Gitter.ViewModels
{
    public class LoginViewModel : ReactiveObject, IRoutableViewModel
    {
        public LoginViewModel(IScreen hostScreen)
        {
            this.HostScreen = hostScreen;
        }

        public IScreen HostScreen { get; private set; }

        public string UrlPathSegment
        {
            get { return "Login"; }
        }
    }
}