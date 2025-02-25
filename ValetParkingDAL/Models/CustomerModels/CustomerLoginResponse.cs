using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CustomerLoginResponse
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsVerified { get; set; }
        public string JwtToken { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

        public string ZipCode { get; set; }
        public string PaypalCustomerId { get; set; }


    }
}