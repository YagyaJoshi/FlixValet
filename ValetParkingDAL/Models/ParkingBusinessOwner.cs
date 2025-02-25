using System;

namespace ValetParkingDAL.Models
{
    public class ParkingBusinessOwner
    {

        public long Id { get; set; }
        public string BusinessTitle { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public int ZipCode { get; set; }

        public long StateId { get; set; }

        public long CountryId { get; set; }
        public string LogoUrl { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
}