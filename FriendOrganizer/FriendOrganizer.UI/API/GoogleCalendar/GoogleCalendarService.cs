using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace FriendOrganizer.UI.API.GoogleCalendar
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly string[] _scopes = { CalendarService.Scope.Calendar };
        private readonly string _applicationName = "Google Calendar API";
        private readonly string _calendarId = "primary";
        private UserCredential credential;

        public GoogleCalendarService()
        {
            AuthenticateAPI();
        }


        private CalendarService Service => new CalendarService(
            new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });

        private void AuthenticateAPI()
        {
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Directory.GetCurrentDirectory();
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }

        public async Task<Google.Apis.Calendar.v3.Data.Event> CreateNewCalendarEvent(string summary, DateTime startTime, 
            DateTime endTime, List<EventAttendee> attendees = null)
        {
            Google.Apis.Calendar.v3.Data.Event newEvent = null;

            if (attendees == null)
            {
                attendees = new List<EventAttendee>();
            }
           await Task.Run((() =>
            {
                newEvent = new Google.Apis.Calendar.v3.Data.Event
                {
                    Summary = summary,
                    Start = new EventDateTime
                    {
                        DateTime = startTime
                    },
                    End = new EventDateTime
                    {

                        DateTime = endTime
                    },
                    Attendees = attendees
                };
                
                newEvent = Service.Events.Insert(newEvent, _calendarId).Execute();
            }));
            return newEvent;
        }

        public async Task<IList<Google.Apis.Calendar.v3.Data.Event>> GetAllEvents()
        {
            return await Task.Run(() => Service.Events.List("primary").ExecuteAsync().Result.Items);
        }

        public async Task RemoveCalendarEvent(string summary)
        {
            //TODO: Add remove to meetingdetailviewmodel
            var eventToRemove = await GetEventBySummary(summary);
            Service.Events.Delete(_calendarId, eventToRemove.Id).Execute();
        }

        private async Task<Google.Apis.Calendar.v3.Data.Event> GetEventBySummary(string summary)
        {
            return await Task.Run(() => Service.Events.List("primary").ExecuteAsync().Result.Items.FirstOrDefault(gm => gm.Summary == summary));
        }
    }
}
