using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class VehicleBookingResponse
    {
        public long BookingId { get; set; }
        public string EntryDate { get; set; }
        public string ExitDate { get; set; }

        public string EnterTime { get; set; }

        public string ExitTime { get; set; }
        public int BookingTypeId { get; set; }

        public DateTime? BookingStartDateTime { get; set; }

        public DateTime? BookingEndDateTime { get; set ; }
    }
}