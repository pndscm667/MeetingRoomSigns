using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using ConferenceRooms.Models;

namespace ConferenceRooms.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<Meeting> allMeetings { get; set; }
        public string nowtime = DateTime.Now.ToString("hh:mm tt");
        public string nowday = DateTime.Today.ToString("dddd, MMMM dd, yyyy ");
        public string nowyear = DateTime.Today.ToString("yyyy");
        public string DisplayName { get; set; }
      
        public void OnGet()
        {
        // If it's debug then let's just give it a test conference room
        // If not then read the parameters out of the URL
#if DEBUG
            string MeetingRoom = "mikesconferenceroom@olympicmedical.org";
            DisplayName = "Mike E's Conference Room";
#else
            string MeetingRoom = Request.Query["meetingroom"].ToString();
            DisplayName = Request.Query["displayname"].ToString();
#endif 

        // Call the Graph API query.  It's going to return a calendar object
        var meeting = getUsersAsync(MeetingRoom).GetAwaiter().GetResult();

            // make a list based on your class in Models
            allMeetings = new List<Meeting>();


            // Loop thru the events that came back from graph and add the pertinent info to your list allMeetings
            // allMeetings is called from the razor page

            foreach (Event meet in meeting)
            {
                allMeetings.Add(new Meeting
                {
                    organizer = meet.Organizer.EmailAddress.Name,
                    subject = meet.Subject,
                    location = meet.Location.DisplayName,
                    // Time comes in from EXO in UTC so convert it back!
                    start = DateTime.Parse(meet.Start.DateTime).ToLocalTime(),
                    end = DateTime.Parse(meet.End.DateTime).ToLocalTime(),

                });
            }
        }
        public async static Task<ICalendarCalendarViewCollectionPage> getUsersAsync(string MeetingRoom)
        {
            //  Set up your auth keys for connecting to MS
            var clientId = "5ee5b7fa-94ec-4ab2-886f-ee1e60f96a29";
            var tenantId = "eadb5e7c-6064-4235-bdb7-5e648530bb77";
            var clientSecret = "GF+GlAm0j1a4QsOad2wZo/xjoRW2AZ/7NhtIAcgKX60=";

            // Instantiate the application
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            // Make the Graph client that we'll be querying
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            // Get a list of calendars for the conference room passed in from onget()
            var calendars = await graphClient
                .Users[MeetingRoom].Calendars
                .Request()
                .GetAsync();

            // Here is the window we'll be grabbing meetings from
            DateTime startingAfter = DateTime.UtcNow;
            DateTime endingBefore = (DateTime.Today.AddDays(1).AddSeconds(-1));
            // Gotta convert to UTC
            endingBefore = endingBefore.ToUniversalTime();
            // Use them to set up query options
            var options = new List<QueryOption>();
            options.Add(new("startDateTime", startingAfter.ToString("o")));
            options.Add(new("endDateTime", endingBefore.ToString("o")));

            // The calendar we want to query is the one called "Calendar" and the only way to do it is thru this bit of Linq
            var calendar = calendars.First(it => it.Name == "Calendar");

            // Now go back and query Graph for all events in the Calendar using the start and end date in options.
            var events = await graphClient
                .Users[MeetingRoom].Calendars[$"{calendar.Id}"].CalendarView
                .Request(options)
                .GetAsync();

            // Finally return the ICalendarCalendarViewCollection to the main routine
            return events;

        }
    }
}
