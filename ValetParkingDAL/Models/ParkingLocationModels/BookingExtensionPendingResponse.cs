using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class BookingExtensionPendingResponse
    {

        public List<ExtensionPendingList> Bookings { get; set; }
        public int Total { get; set; }
    }
    public class ExtensionPendingList
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string NumberPlate { get; set; }
    }
}