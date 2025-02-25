using System;
using System.Collections.Generic;
using ValetParkingDAL.Models.ParkingLocationModels;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class PreBookingDetailsResponse
    {
        public ParkingLocationDetails ParkingLocationDetails { get; set; }
        public CustomerDetailsResponse CustomerDetails { get; set; }
    }
    public class ParkingLocationDetails
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }
        public string StateCode { get; set; }

        public string Country { get; set; }

        public string CountryCode { get; set; }

        public string ZipCode { get; set; }
        public string LocationPic { get; set; }

        public string Mobile { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public int No_of_Spaces { get; set; }

        public string Currency { get; set; }
        public string TimeZoneId { get; set; }
        public string TimeZone { get; set; }
        public string Instructions { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string LocationName { get; set; }
        public decimal OverSizedChargesRegular { get; set; }

        public decimal OverSizedChargesMonthly { get; set; }
        public long ParkingBusinessOwnerId { get; set; }
        public decimal Tax { get; set; }
        public string MobileCode { get; set; }
        public DateTime StartDate { get; set; }
        public decimal ConvenienceFee { get; set; }
        public List<List<ParkingLocationTimingRequest>> ParkingTimings { get; set; }
    }
}