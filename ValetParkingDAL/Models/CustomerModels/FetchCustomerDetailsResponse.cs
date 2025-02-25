using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class FetchCustomerDetailsResponse
    {
        public long CustomerId { get; set; }
        public List<CustomerVehicleList> CustomerVehicles { get; set; }
        public ParkingLocationDetailsResponse ParkingLocationDetails { get; set; }
    }
    public class ParkingLocationDetailsResponse
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }

        public string Currency { get; set; }

        public string Symbol { get; set; }

        public string LocationPic { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PerHourRate { get; set; }
        public decimal? OverSizedCharges { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        [JsonIgnore]
        public decimal OverSizedChargesMonthly { get; set; }
        [JsonIgnore]
        public decimal OverSizedChargesRegular { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsOverSizedVehicle { get; set; }
        public double MaxDurationofSlab { get; set; }
        public decimal MaxRateofSlab { get; set; }
        public string LogoUrl { get; set; }
        public decimal ConvenienceFee { get; set; }
        public decimal FinalAmountWithConvenienceFee { get; set; }
    }

    public class CustomerVehicleList
    {
        public long? Id { get; set; }
        public long? CustomerInfoId { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
        public int? VehicleTypeId { get; set; }
    }
}