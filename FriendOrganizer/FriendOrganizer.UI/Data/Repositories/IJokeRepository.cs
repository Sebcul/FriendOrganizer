using System.Threading.Tasks;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Repositories
{
    public interface IJokeRepository : IGenericRepository<Joke>
    {
        Task<Joke> AddJokeAsync(Joke joke);
    }
}