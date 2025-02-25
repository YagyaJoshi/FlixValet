using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class ParkingBusinessOwnerResponse
    {
        public List<PBusinessOwner> ParkingBusinessOwner { get; set; }
        public int Total { get; set; }

    }

    public class PBusinessOwner
    {
        public long Id { get; set; }
        public string BusinessTitle { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public string MobileCode { get; set; }
    }

    public class ParkingBusinessOwnerDetails
    {
        public long Id { get; set; }
        public string Email { get; set; }  
    }
}