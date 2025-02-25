using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class WhiteListCustomers
    {
        public long WhiteListCustomerId { get; set; }
        [Required]
        public long ParkingBusinessOwnerId { get; set; }
        [Required]
        public string NumberPlate { get; set; }
        public bool IsActive { get; set; }
        public string VehicleModal { get; set; }

        public int? VehicleTypeId { get; set; }
        public int? VehicleColorId { get; set; }
        public long? VehicleManufacturerId { get; set; }
        [StringLength(3)]
        public string StateCode { get; set; }
        [StringLength(3)]
        public string CountryCode { get; set; }
        public long CreatedBy { get; set; }
        public long ModifyBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}