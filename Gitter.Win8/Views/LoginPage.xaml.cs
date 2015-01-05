using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.OnlineId;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Akavache;
using Gitter.ViewModels;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Gitter.Win8.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : IViewFor<LoginViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(LoginViewModel), typeof(LoginPage), new PropertyMetadata(null));

        public LoginPage()
        {
            this.InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel)
                .Select(_ => this.Authenticate().ToObservable())
                .Concat()
                .Subscribe();
        }


        public LoginViewModel ViewModel
        {
            get { return (LoginViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (LoginViewModel)value; }
        }

        private async Task Authenticate()
        {
            //
            // Generate a unique state string to check for forgeries
            //
            var chars = new char[16];
            var rand = new Random();
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)rand.Next((int)'a', (int)'z' + 1);
            }
           
            string requestState = new string(chars);

            var url = new Uri(string.Format(
                "{0}?client_id={1}&redirect_uri={2}&response_type={3}&scope={4}&state={5}",
                "https://gitter.im/login/oauth/authorize",
                "a0fc459712567a41ccd5fb8bbfbf35ce0ea6cb56",
                "http://oauth.gitter.flagbug.com",
                "code",
                String.Empty,
                requestState));

            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, url, new Uri("http://oauth.gitter.flagbug.com"));

            if (result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var decoded = ParseQueryString(result.ResponseData);
                string code = decoded["code"];
                
                IDictionary<string, string> authResult = await this.RequestAccessTokenAsync(code);

                await BlobCache.Secure.SaveLogin("Gitter", authResult["access_token"], "Gitter");

                await this.ViewModel.HostScreen.Router.Navigate.ExecuteAsync(new RoomsViewModel());
            }
        }

        private static IDictionary<string, string> ParseQueryString(string s)
        {
            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            var dictionary = new Dictionary<string, string>();

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] strings = Regex.Split(vp, "=");
                dictionary.Add(strings[0], strings.Length == 2 ? WebUtility.UrlDecode(strings[1]) : string.Empty);
            }

            return dictionary;
        }

        /// <summary>
        /// Asynchronously makes a request to the access token URL with the given parameters.
        /// </summary>
        /// <param name="queryValues">The parameters to make the request with.</param>
        /// <returns>The data provided in the response to the access token request.</returns>
        private async Task<IDictionary<string, string>> RequestAccessTokenAsync(IDictionary<string, string> queryValues)
        {
            string query = await new FormUrlEncodedContent(queryValues).ReadAsStringAsync();

            using (var client = new HttpClient())
            {
                var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage response = await client.PostAsync("https://gitter.im/login/oauth/token", content);

                string responseContent = await response.Content.ReadAsStringAsync();

                IDictionary<string, JToken> data = JObject.Parse(responseContent);

                if (data["error"] != null)
                {
                    throw new Exception("Error authenticating: " + data["error"]);
                }

                if (data["access_token"] != null)
                {
                    return data.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
                }

                throw new Exception("Expected access_token in access token response, but did not receive one.");
            }
        }

        /// <summary>
        /// Asynchronously requests an access token with an authorization <paramref name="code" /> .
        /// </summary>
        /// <returns>A dictionary of data returned from the authorization request.</returns>
        /// <param name="code">The authorization code.</param>
        /// <remarks>Implements: http://tools.ietf.org/html/rfc6749#section-4.1</remarks>
        private Task<IDictionary<string, string>> RequestAccessTokenAsync(string code)
        {
            var queryValues = new Dictionary<string, string> {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", "http://oauth.gitter.flagbug.com" },
                { "client_id", "a0fc459712567a41ccd5fb8bbfbf35ce0ea6cb56" },
            };

            queryValues["client_secret"] = "fbf16dedfed97d1d059604cedaf8bfc2d69d3cbe";

            return RequestAccessTokenAsync(queryValues);
        }
    }
}