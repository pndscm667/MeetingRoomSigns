using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
//using Microsoft.Graph.Auth;
//using Microsoft.Identity.Client;
using ConferenceRooms.Models;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;


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
        public string indexNumber { get; set; }
        public string TodaysQuote { get; set; }
        public string TodaysAuthor { get; set; }
       
        public int quoteindex()
        { 
            // Gotta get a quote
            TimeSpan tspan = DateTime.Today - new DateTime(1970, 1, 1);
            // TimeSpan tspan = DateTime.Parse("3/18/2024") - new DateTime(1970, 1, 1);
            int daysSinceEpoch = (int)tspan.TotalDays;
            int index = (daysSinceEpoch % 330) + 1;
            return index;
        }

        // Get the Quote of The Day from SQLPROD4
        public (string, string) QOTD()
        {

            var connectionString = "Server=SQLPROD4;Database=QuoteOfTheDay;User Id=QOTD_reader;password=Wzrkg0}DcbxCjmPt[}0v;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;";
            using SqlConnection connection = new SqlConnection(connectionString);
            {
                

                // First command to populate meeting info grid

                using SqlCommand cmd = new SqlCommand("getQuote");
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("indexNumber", quoteindex().ToString());

                    cmd.Connection = connection;
                    connection.Open();

                    SqlDataReader quoteDetails = cmd.ExecuteReader();
                    DataTable datatablequote = new DataTable();

                    datatablequote.Load(quoteDetails);

                    connection.Close();
                    return (datatablequote.Rows[0][0].ToString(), datatablequote.Rows[0][1].ToString());

                }
            }

        }


        public void OnGet()
        {
            // If it's debug then let's just give it a test conference room
            // If not then read the parameters out of the URL
#if DEBUG
            string MeetingRoom = "fairshterconferenceroom@olympicmedical.org";
            DisplayName = "Fairshter Conference Room";
#else
            string MeetingRoom = Request.Query["meetingroom"].ToString();
            DisplayName = Request.Query["displayname"].ToString();
#endif
            TodaysQuote = QOTD().Item1;
            TodaysAuthor = QOTD().Item2;

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
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientId = "5ee5b7fa-94ec-4ab2-886f-ee1e60f96a29";
            var tenantId = "eadb5e7c-6064-4235-bdb7-5e648530bb77";
            var clientSecret = "GF+GlAm0j1a4QsOad2wZo/xjoRW2AZ/7NhtIAcgKX60=";

            // using Azure.Identity;
            var opts = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };
                     
            // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, opts);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

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
