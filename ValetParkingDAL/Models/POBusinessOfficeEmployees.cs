using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class POBusinessOfficeEmployees
    {
        public long BusinessOfficeEmployeeId { get; set; }
        public long BusinessOfficeId { get; set; }
        public long CustomerVehicleId { get; set; }
        public int OfficeDuration { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public long ModifyBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

    }

    public class POBusinessOfficeEmployeeInput
    {
        public long? CustomerId { get; set; }
        public CustomerDetail Customer { get; set; }
        public string PhoneNumber { get; set; }
        public long BusinessOfficeEmployeeId { get; set; }
        public long BusinessOfficeId { get; set; }
        public long CustomerVehicleId { get; set; }
        public CustomerVehicleDetails CustomerVehicle {  get; set; }
        public int OfficeDuration { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public long ModifyBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

    }

    public class CustomerVehicleDetails
    {

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

    public class CustomerDetail
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
    }
}