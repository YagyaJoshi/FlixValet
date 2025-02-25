using System;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class BookingDetailResponse
    {
        public long Id { get; set; }
        public long? EnterExitId { get; set; }

        public long ParkingLocationId { get; set; }

        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string MobileCode { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ProfilePic { get; set; }
        [JsonIgnore]
        public decimal Charges { get; set; }
        public decimal BookingAmount { get; set; }
        public decimal TotalCharges { get; set; }
        public string OverStayHours { get; set; }
        public double? TotalOverStayDuration { get; set; }
        public decimal? TotalOverStayCharges { get; set; }
        public decimal OverweightCharges { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }

        public bool IsAdditionalChargesApplied { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Duration { get; set; }
        public string NumberPlate { get; set; }

        public string VehicleModal { get; set; }
        public string VehicleManufacturer { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleState { get; set; }
        public string Notes { get; set; }
        public string EntryDate { get; set; }
        public string ExitDate { get; set; }

        public string EnterTime { get; set; }
        public string ExitTime { get; set; }

        public long? CustomerInfoId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public bool IsGuest { get; set; }
        public string MaxStay { get; set; }
        public string BookingMessage { get; set; }
        public string Symbol { get; set; }

        public string Currency { get; set; }

        public bool IsEarlyBirdOfferApplied { get; set; }

        public DateTime? LastBookingDate { get; set; }

        public DateTime? StartBookingDate { get; set; }


        public string BookingType { get; set; }
        public string TimeZone { get; set; }

        public double? MaxDurationofSlab { get; set; }

        public decimal? MaxRateofSlab { get; set; }

        public string MonthlyPassMessage { get; set; }
        public bool HasPaymentSetup { get; set; }

        [JsonIgnore]
        public decimal PreviousBookingAmount { get; set; }

        public ParkingOwnerInfo ParkingOwnerInfo { get; set; }

        public int BookingTypeId { get; set; }

        public decimal ConvenienceFee { get; set; }
        public int? BookingCategoryId { get; set; }
        public string BookingNotes { get; set; }
        public string PaymentMode { get; set; }

        public string PaypalCustomerId { get; set; }

        public string LocationId { get; set; }
        public string AccessToken { get; set; }
        public string PaymentMethod { get; set; }

        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool IsProduction { get; set; }
        public string ApplicationId { get; set; }
        public decimal? TaxAmountWithConvenienceFee { get; set; }
        public decimal FinalAmountWithConvenienceFee { get; set; }
        public decimal UnpaidAmountWithConvenienceFee { get; set; }

        public decimal LocationConvenienceFee { get; set; }
    }

    public class ParkingOwnerInfo
    {
        public long ParkingBusinessOwnerId { get; set; }
        public string BusinessTitle { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string State { get; set; }

        public string StateCode { get; set; }

        public string Country { get; set; }

        public string CountryCode { get; set; }
        public string LogoUrl { get; set; }

    }
}

