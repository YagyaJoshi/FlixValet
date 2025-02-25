using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.NumberPlateRecogModels
{
    public class DetectedVehiclesInfo
    {
        public List<BookingStatus> BookingStatus { get; set; }

        public LocationCameraInfo LocCameraInfo { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}