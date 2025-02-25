using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class LoginResponse
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic {get;set;}
        public string Mobile { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsVerified { get; set; }
        public string JwtToken { get; set; }
        public long? ParkingBusinessOwnerId{get;set;}
        public bool IsSuperAdmin {get;set;}
        public List<long> ParkingLocations { get; set; }
        public string BusinessTitle { get; set; }
        public string LogoUrl { get; set; }

        public long? BusinessOfficeId { get; set; }

    }
}