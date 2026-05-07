using Microsoft.EntityFrameworkCore;

namespace EventEase.Models
{
    [Keyless]
    public class BookingDetailsView
    {
        public int BookingID {  get; set; }
        public DateTime BookingDate {  get; set; }
        public int EventID {  get; set; }
        public string EventName {  get; set; }
        public DateTime EventDate {  get; set; }
        public string Description { get; set;  }
        public int VenueID {  get; set; }
        public string VenueName {  get; set; }
        public string Location {  get; set; }
        public int Capacity { get; set; }
        public string? ImageURL {  get; set; }
        
    }
}
