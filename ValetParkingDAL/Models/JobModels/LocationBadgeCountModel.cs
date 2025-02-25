using System;
using System.Collections.Generic;
using System.Text;

namespace ValetParkingDAL.Models.JobModels
{
    public class LocationBadgeCountModel
    {
        public long ParkingLocationId { get; set; }

        //  public long CustomerId { get; set; }
        //  public long CustomerBookingId { get; set; }

        public long BadgeCount { get; set; }

        // public string NumberPlate { get; set; }
    }
}
