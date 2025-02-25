using System;

namespace ValetParkingDAL.Models
{
    public class ParkingEnterExitRecords
    {
        public long Id { get; set; }
        public long ParkingLocationId { get; set; }
        public long CustomerBookingId { get; set; }
        public long? CustomerInfoId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public string EntryDate { get; set; }
        public string EnterTime { get; set; }
        public string ExitTime { get; set; }
        public string Notes { get; set; }
        public string ExitDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}