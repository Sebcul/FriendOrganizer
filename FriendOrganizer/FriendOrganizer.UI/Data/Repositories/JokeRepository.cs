using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Repositories
{
    public class JokeRepository : GenericRepository<Joke, FriendOrganizerDbContext>, IJokeRepository
    {
        public JokeRepository(FriendOrganizerDbContext context) : base(context)
        {
        }


        private async Task<bool> JokeExistsAsync(Joke joke)
        {
            return await Context.Jokes.AnyAsync(j =>
                j.Setup == joke.Setup && j.Punchline == joke.Punchline && j.Type == joke.Type);
        }

        public async Task<Joke> AddJokeAsync(Joke joke)
        {
            if (await JokeExistsAsync(joke))
            {
                return await Context.Set<Joke>().FirstOrDefaultAsync(j =>
                    j.Setup == joke.Setup && j.Punchline == joke.Punchline && j.Type == joke.Type);
            }

            Context.Set<Joke>().Add(joke);
            return joke;
        }
    }
}
