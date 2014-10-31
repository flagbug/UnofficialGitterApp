using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gitter.Models;
using ModernHttpClient;
using Newtonsoft.Json.Linq;
using Refit;

namespace Gitter
{
    [Headers("Accept: application/json")]
    public interface IGitterApi
    {
        [Get("/rooms/{id}/chatMessages")]
        IObservable<IReadOnlyList<Message>> GetMessages([AliasAs("id")] string roomId, [Header("Authorization")] string accessToken);

        [Get("/rooms")]
        Task<IReadOnlyList<Room>> GetRooms([Header("Authorization")] string accessToken);
    }

    public interface IGitterStreamingApi
    {
        IObservable<Message> ObserveMessages(string roomId, string accessToken);
    }

    public class GitterStreamingApi : IGitterStreamingApi
    {
        public IObservable<Message> ObserveMessages(string roomId, string accessToken)
        {
            string url = string.Format("https://stream.gitter.im/v1/rooms/{0}/chatMessages", roomId);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client.GetStreamAsync(url).ToObservable()
                .Select(x => Observable.FromAsync(() => ReadLineUntil(x, '\r')).Repeat())
                .Concat()
                .Select(x => JObject.Parse(x).ToObject<Message>())
                .Finally(() => client.Dispose());
        }

        private Task<string> ReadLineUntil(Stream stream, char delimiter)
        {
            var stringBuilder = new StringBuilder();
            var reader = new StreamReader(stream, Encoding.UTF8);

            return Task.Run(() =>
            {
                char read;
                while ((read = (char)reader.Read()) > 0 && read != delimiter)
                {
                    stringBuilder.Append(read);
                }

                return stringBuilder.ToString();
            });
        }
    }
}