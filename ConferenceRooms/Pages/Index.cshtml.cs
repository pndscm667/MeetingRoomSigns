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
        public string nowday = DateTime.Today.ToString("MMMM d");

        public void OnGet()
        {
            // Call the Graph API query.  It's going to return a calendar object
            var meeting = getUsersAsync().GetAwaiter().GetResult();

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
                    start = DateTime.Parse(meet.Start.DateTime).ToLocalTime(),
                    end = DateTime.Parse(meet.End.DateTime).ToLocalTime(),

                });
            }
        }
        public async static Task<ICalendarCalendarViewCollectionPage> getUsersAsync()
        {
            //  Set up your auth keys for connecting to MS
            var clientId = "5ee5b7fa-94ec-4ab2-886f-ee1e60f96a29";
            var tenantId = "eadb5e7c-6064-4235-bdb7-5e648530bb77";
            var clientSecret = "r.2RD~XsdIJZ9t6SA_szTG5bb0_0v27Uzz";

            // Instantiate the application
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            // Make the Graph client that we'll be querying
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            // Choose a user to get the calendar for
            string userName = "testconfroom@olympicmedical.org";

            // Get a list of calendars for that user
            var calendars = await graphClient
                .Users[userName].Calendars
                .Request()
                .GetAsync();

            // Here is the window we'll be grabbing meetings from
            var startingAfter = DateTime.UtcNow;
            var endingBefore = DateTime.UtcNow.AddDays(30);

            // Use them to set up query options
            var options = new List<QueryOption>();
            options.Add(new("startDateTime", startingAfter.ToString("o")));
            options.Add(new("endDateTime", endingBefore.ToString("o")));

            // The calendar we want to query is the one called "Calendar" and the only way to do it is thru this bit of Linq
            var calendar = calendars.First(it => it.Name == "Calendar");

            // Now go back and query Graph for all events in the Calendar using the start and end date in options.
            var events = await graphClient
                .Users[userName].Calendars[$"{calendar.Id}"].CalendarView
                .Request(options)
                .GetAsync();

            // Finally return the ICalendarCalendarViewCollection to the main routine
            return events;

        }
    }
}
