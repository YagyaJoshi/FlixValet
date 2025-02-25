using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.UserModels
{
    public class DamageVehicleDetailsResponse
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long ParkingLocationId { get; set; }

        public long CustomerBookingId { get; set; }
        public long? CustomerId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public string ValetName { get; set; }
        public string CustomerName { get; set; }
        public string Notes { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }

        public DateTime ReportedDate { get; set; }
        public List<DamageVehicleImages> Images { get; set; }
    }
    public class DamageVehicleImages
    {
        public long DamageVehicleId { get; set; }
        public string ImageURL { get; set; }

    }
}