using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ValetParkingDAL.Models.ParkingLocationModels;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class ParkingLocDetailsResponse
    {
        public long Id { get; set; }
        public string LocationName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }
        public string LocationPic { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string MobileCode { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PerHourRate { get; set; }
        public decimal? OverSizedCharges { get; set; }

        [JsonIgnore]
        public decimal OverSizedChargesMonthly { get; set; }
        [JsonIgnore]
        public decimal OverSizedChargesRegular { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal FinalAmount { get; set; }
        public bool IsOverSizedVehicle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BookingMessage { get; set; }
        public string MaxStay { get; set; }

        public CustomerDetails CustomerDetails { get; set; }
        public bool IsEarlyBirdOfferApplicable { get; set; }
        public bool IsNightFareOfferApplicable { get; set; }
        public EarlyBirdInfo EarlyBirdInfo { get; set; }
        public NightFareInfo NightFareInfo { get; set; }

        public decimal? EarlyBirdTaxAmount { get; set; }
        public decimal? EarlyBirdFinalAmount { get; set; }

        public decimal? NightFareTaxAmount { get; set; }
        public decimal? NightFareFinalAmount { get; set; }
        public List<List<ParkingLocationTimingRequest>> ParkingTimings { get; set; }
        public string PaymentMethod { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string AccessToken { get; set; }

        public bool IsProduction { get; set; }
        public string ApplicationId { get; set; }
        public string LocationId { get; set; }
        public double MaxDurationofSlab { get; set; }
        public decimal MaxRateofSlab { get; set; }

        public string Currency { get; set; }

        public string Symbol { get; set; }

        public string BusinessTitle { get; set; }
        public string LogoUrl { get; set; }

        public decimal ConvenienceFee { get; set; }


    }

    public class CustomerDetails
    {
        public long CustomerId { get; set; }
        public string Address { get; set; }
        public List<VehicleDetails> CustomerVehicles { get; set; }

    }

    public class VehicleDetails
    {
        public long? Id { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
    }
}