using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingDAL.Models
{
    public class VehicleDamage
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        [Required]
        public long ParkingLocationId { get; set; }
        [Required]
        public long CustomerBookingId { get; set; }
        public long? CustomerId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public string Notes { get; set; }
        public List<DamageVehicleImages> Images { get; set; }
    }

}