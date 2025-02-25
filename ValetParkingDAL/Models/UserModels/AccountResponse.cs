using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class AccountResponse
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic {get;set;}
        public string Mobile { get; set; }
        public string Email { get; set; }
        public List<Role> Roles { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsVerified { get; set; }
        public bool SuperAdmin {get;set;}
        
    }
}