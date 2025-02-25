using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestDetailsRequest
    {

        public string Mobile { get; set; }
        public string Email { get; set; }


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