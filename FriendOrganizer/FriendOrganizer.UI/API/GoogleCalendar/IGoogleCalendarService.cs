using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;

namespace FriendOrganizer.UI.API.GoogleCalendar
{
    public interface IGoogleCalendarService
    {
        Task<Google.Apis.Calendar.v3.Data.Event> CreateNewCalendarEvent(string summary, DateTime startTime, DateTime endTime, List<EventAttendee> attendees = null);
        Task<IList<Google.Apis.Calendar.v3.Data.Event>> GetAllEvents();
        //Task<Google.Apis.Calendar.v3.Data.Event> GetEventBySummary(string summary);
    }
}