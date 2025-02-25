using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CustomerDetailsResponse
    {

        public long UserId { get; set; }
        public long CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }
        public string ProfilePic { get; set; }
        public char Gender { get; set; }
        public string MobileCode { get; set; }
        public List<VehicleDetails> CustomerVehicles { get; set; }
    }
}