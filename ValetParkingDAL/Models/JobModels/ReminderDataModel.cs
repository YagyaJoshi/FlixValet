using System;

namespace ValetParkingDAL.Models.JobModels
{
    public class ReminderDataModel
    {
        public long CustomerBookingId { get; set; }
        public long ParkingLocationId { get; set; }
        public long CustomerId { get; set; }
        public long CustomerVehicleId { get; set; }
        public string NumberPlate { get; set; }
        public string DeviceToken { get; set; }
        public string BrowserDeviceToken { get; set; }
        public string Mobile { get; set; }
        public long CustomerBadgeCount { get; set; }
        public DateTime BookingEndTime { get; set; }
    }
}