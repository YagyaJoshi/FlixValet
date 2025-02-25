using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class InspectCheckInResponse
    {
        public long? Id { get; set; }
        public DateTime? CheckInTime { get; set; }
        public bool IsCheckedIn { get; set; }
        public string Message { get; set; }

    }
}