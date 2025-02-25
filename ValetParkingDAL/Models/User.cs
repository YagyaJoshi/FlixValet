using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models
{
    public class User
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Mobile { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
        public string AppVersionCode { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? Verified { get; set; }
        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public DateTime? PasswordReset { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public string OTPToken { get; set; }
        public char Gender { get; set; } = 'U';
        public bool IsActive { get; set; }
        public long? ParkingBusinessOwnerId { get; set; }
        public string BusinessTitle { get; set; }
        public string LogoUrl { get; set; }
        public List<Role> Roles { get; set; }
        public string MobileCode { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

        public string ZipCode { get; set; }
        public string PaypalCustomerId { get; set; }
        public long? BusinessOfficeId {  get; set; }
    }
}