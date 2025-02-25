using System;
using System.Collections.Generic;
using System.Text;

namespace ValetParkingDAL.Models.JobModels
{
    public class CustomerInfoModel
    {

        public long CustomerId { get; set; }

        public long ParkingLocationId { get; set; }

        public long CustomerBookingId { get; set; }
        public string DeviceToken { get; set; }
        public string BrowserDeviceToken { get; set; }
        public string Mobile { get; set; }
        public string NumberPlate { get; set; }
        public long BadgeCount { get; set; }

    }
}
