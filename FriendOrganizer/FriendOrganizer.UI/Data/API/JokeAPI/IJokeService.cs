using System.Threading.Tasks;
using FriendOrganizer.UI.Wrapper;

namespace FriendOrganizer.UI.Data.API.JokeAPI
{
    public interface IJokeService
    {
        Task<JokeWrapper> GetRandomJoke();
    }
}