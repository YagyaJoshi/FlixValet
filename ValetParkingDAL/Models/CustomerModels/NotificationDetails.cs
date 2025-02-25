namespace ValetParkingDAL.Models.CustomerModels
{
    public class NotificationDetails
    {
        public string[] DeviceTokens { get; set; }

        public long CustomerId { get; set; }

        public long ParkingLocationId { get; set; }

        public string NumberPlate { get; set; }

        public string TimeZone { get; set; }

        public long BadgeCount { get; set; }
    }
}