using System;
namespace ConferenceRooms.Models
{
    public class Meeting
    {
        public string subject { get; set; }
        public string organizer { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string location { get; set; }
               
    }
}
