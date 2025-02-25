using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class CreateUserRequest
    {
        public long UserId { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        [Required]
        [StringLength(50)]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        public string Email { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public char Gender { get; set; } ='U';
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public int[] Role { get; set; }
        public long ParkingBusinessOwnerId { get; set; }
        public long[] ParkingLocations { get; set; }
        public string LicenseUrl { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public string MobileCode { get; set; }
    }
}