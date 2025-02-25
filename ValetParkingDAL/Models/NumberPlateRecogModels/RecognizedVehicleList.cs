using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.NumberPlateRecogModels
{
    public class RecognizedVehicleList
    {
        public List<RecognizedVehicle> ListVehicles { get; set; }
    }
    public class RecognizedVehicle
    {

        public long Id { get; set; }
        public string NumberPlate { get; set; }
        public bool ShowGuestBookingButton { get; set; }
        public bool ShowCheckInButton { get; set; }
        public bool ShowCheckOutButton { get; set; }
        public bool IsBookingFound { get; set; }
        public long? CustomerBookingId { get; set; }
        public long? CustomerInfoId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public bool HasEntered { get; set; }
        public DateTime ReportedTime { get; set; }

    }
}