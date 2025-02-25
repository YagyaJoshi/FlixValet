using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class PushNotificationModel
    {
        public long CustomerId { get; set; }
        public long ParkingLocationId { get; set; }
        public string TimeZone { get; set; }
        public string LocationName { get; set; }
        public string CustomerName { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
        public DateTime EnterDate { get; set; }
        public string EnterTime { get; set; }
        public DateTime ExitDate { get; set; }
        public string ExitTime { get; set; }
        public decimal BookingAmount { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string NotificationMessage { get; set; }

        public long CustomerBookingId { get; set; }
        public long BadgeCount { get; set; }

        [JsonIgnore]
        public List<string> DeviceTokens { get; set; }
    }
}