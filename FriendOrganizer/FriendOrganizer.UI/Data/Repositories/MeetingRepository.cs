﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Repositories
{
    public class MeetingRepository : GenericRepository<Meeting, FriendOrganizerDbContext>, IMeetingRepository
    {
        protected MeetingRepository(FriendOrganizerDbContext context) : base(context)
        {
            
        }

        public override async Task<Meeting> GetByIdAsync(int id)
        {
            return await Context.Meetings.Include(f => f.Friends).SingleAsync(m => m.Id == id);
        }
    }
}
