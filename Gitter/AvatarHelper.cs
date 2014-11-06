using System;
using System.Net.Http;
using System.Reactive.Linq;
using Akavache;
using Fusillade;

namespace Gitter
{
    public static class AvatarHelper
    {
        public static IObservable<byte[]> LoadAvatar(Uri avatarSource)
        {
            if (avatarSource == null)
                throw new ArgumentNullException("avatarSource");

            return BlobCache.LocalMachine.GetOrFetchObject(avatarSource.ToString(), () => GetData(avatarSource), DateTimeOffset.Now + TimeSpan.FromDays(1));
        }

        private static IObservable<byte[]> GetData(Uri requestUrl)
        {
            return Observable.Using(() => new HttpClient(NetCache.Background),
                client => Observable.FromAsync(ct => client.GetAsync(requestUrl, ct)))
                    .SelectMany(message => message.Content.ReadAsByteArrayAsync());
        }
    }
}