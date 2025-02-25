using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class GuestUser
    {
        public long Id { get; set; }
        [Required]
        public string Phone { get; set; }

        public string VehicleName { get; set; }


        public string VehicleModal { get; set; }

        public string VehicleNumber { get; set; }

        public long VehicleColorId { get; set; }

        public long VehicleStateId { get; set; }

        public long VehicleManufacturerId { get; set; }

        public int VehicleTypeId { get; set; }
        public string OTP { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}