using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace ValetParkingDAL.Models
{
    public class CustomerVehicles
    {

        public long Id { get; set; }
        [Required]
        public long CustomerInfoId { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "Maximum length should be 20")]
        public string NumberPlate { get; set; }

        [StringLength(50)]
        public string VehicleModal { get; set; }

        public int? VehicleTypeId { get; set; }
        public int? VehicleColorId { get; set; }
        public long? VehicleManufacturerId { get; set; }
        [StringLength(3)]
        public string StateCode { get; set; }
        [StringLength(3)]

        public string CountryCode { get; set; }

    }
}