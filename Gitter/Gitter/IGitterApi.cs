using System.Collections.Generic;
using System.Threading.Tasks;
using Gitter.Models;
using Refit;

namespace Gitter
{
    public interface IGitterApi
    {
        [Get("/rooms")]
        Task<IReadOnlyList<Room>> GetRooms([Header("Authorization")] string accessToken);
    }
}