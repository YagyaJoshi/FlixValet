using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class ParkingOwnerRequest
    {
        public long ParkingBusinessOwnerId { get; set; }
        [Required]
        [StringLength(500)]
        public string BusinessTitle { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
        public string OTPToken { get; set; }
        public char Gender { get; set; } ='U';
        [Required]
        [StringLength(100)]
        public string Address { get; set; }
        [Required]
        [StringLength(100)]
        public string City { get; set; }
        [Required]
        public string Mobile { get; set; }

        public string MobileCode { get; set; }
        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        public string Email { get; set; }

        public string State { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "StateCode is required")]
        public string StateCode { get; set; }

        public string Country { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Zip code length must be between 5 to 10")]
        [Range(0, Int64.MaxValue, ErrorMessage = "Please enter a valid Zipcode")]
        public string ZipCode { get; set; }
        public string LogoUrl { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long CreatedBy { get; set; }
        [Required]
        public bool IsActive { get; set; }

    }
}