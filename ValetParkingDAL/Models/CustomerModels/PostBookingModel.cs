using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class PostBookingModel
    {
        public long BookingId { get; set; }

        public long CustomerId { get; set; }

        public string NumberPlate { get; set; }

        public string Symbol { get; set; }

        public BadgeCount BadgeCounts { get; set; }

        public DeviceToken DeviceTokens { get; set; }

        public string CustomerMessage { get; set; }

        public string CustomerTitle { get; set; }

        public string StaffMessage { get; set; }

        public string StaffTitle { get; set; }

        public List<string> ListOwnerSupervisors { get; set; }

        public string LocationName { get; set; }
        public string ElectronicPaymentMessage { get; set; }
    }


    public class BadgeCount
    {
        public long ValetBadgeCount { get; set; }
        public long CustomerBadgeCount { get; set; }
    }

    public class DeviceToken
    {

        public List<string> StaffTokens { get; set; }

        public CustomerToken CustomerTokens { get; set; }

    }

    public class CustomerToken
    {
        public string DeviceToken { get; set; }

        public string BrowserDeviceToken { get; set; }
    }


}