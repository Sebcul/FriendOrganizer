using System.Threading.Tasks;
using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;
using System.Data.Entity;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FriendOrganizer.UI.Data.Repositories
{
    public class MeetingRepository : GenericRepository<Meeting, FriendOrganizerDbContext>,
        IMeetingRepository
    {
        public MeetingRepository(FriendOrganizerDbContext context) : base(context)
        {
        }

        public async override Task<Meeting> GetByIdAsync(int id)
        {
            return await Context.Meetings
                .Include(m => m.Friends).Include(m => m.Jokes)
                .SingleAsync(m => m.Id == id);
        }

        public async Task<List<Friend>> GetAllFriendsAsync()
        {
            return await Context.Set<Friend>()
                .ToListAsync();
        }

        public async Task ReloadFriendAsync(int friendId)
        {
            var dbEntityEntry = Context.ChangeTracker.Entries<Friend>()
                .SingleOrDefault(db => db.Entity.Id == friendId);
            if (dbEntityEntry != null)
            {
                await dbEntityEntry.ReloadAsync();
            }
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

        public async Task<IEnumerable<Joke>> GetAllJokesForMeetingAsync(int meetingId)
        {
            var meeting = await Context.Meetings
                .Include(m => m.Friends)
                .Include(m => m.Jokes)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            return meeting.Jokes;
        }

        private async Task<bool> JokeExistsAsync(Joke joke)
        {
            return await Context.Jokes.AnyAsync(j =>
                j.Setup == joke.Setup && j.Punchline == joke.Punchline && j.Type == joke.Type);
        }
    }
}
