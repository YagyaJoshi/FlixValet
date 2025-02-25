using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class RequestParkingLocations
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string LocationPic { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int No_of_Spaces { get; set; }
        public string LocationName { get; set; }

        public decimal Distance { get; set; }

        public string Mobile { get; set; }
        public string MobileCode { get; set; }
        public long ParkingBusinessOwnerId { get; set; }
        public string Currency { get; set; }
        public string Symbol { get; set; }
        public List<RequestParkingRates> Rates { get; set; }

        public double TotalHours { get; set; }

        public decimal TotalCharges { get; set; }
    }
}