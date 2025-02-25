using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingDAL.Models.CustomerModels
{
	public class BookingRequest
	{
		public long Id { get; set; }
		public long? CustomerId { get; set; }
		public long? CustomerVehicleId { get; set; }
		public long ParkingLocationId { get; set; }

		[Required]
		public string BookingType { get; set; }

		[Required]
		public DateTime StartDate { get; set; }
		[Required]
		public DateTime EndDate { get; set; }
		[Required]
		[ValidateDateTime]
		public string StartTime { get; set; }
		[Required]
		[ValidateDateTime]
		public string EndTime { get; set; }
		[Required]
		public string TimeZoneId { get; set; }
		public string TimeZone { get; set; }

		public bool IsFullTimeBooking { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public string PaymentMode { get; set; } 
		public long? UserId { get; set; }
		public bool IsEarlyBirdOfferApplied { get; set; }
        public bool IsNightFareOfferApplied { get; set; }
        public long? EarlyBirdId { get; set; }
        public long? NightFareId { get; set; }
		public decimal? EarlyBirdAmount { get; set; }
		public decimal? EarlyBirdTaxAmount { get; set; }
		public decimal? EarlyBirdFinalAmount { get; set; }
        public decimal? NightFareAmount { get; set; }
        public decimal? NightFareTaxAmount { get; set; }
        public decimal? NightFareFinalAmount { get; set; }
        public bool IsPaymentFromCustomerSite { get; set; }
		public StripeModel StripeInfo { get; set; }

		public SquareupModel SquareupInfo { get; set; }
		public PaypalPaymentModel PaypalInfo { get; set; }
		public decimal PerHourRate { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal? OverSizedCharges { get; set; }
		public decimal FinalAmount { get; set; }
		public decimal? TaxAmount { get; set; }
		public double? MaxDurationofSlab { get; set; }
		public decimal? MaxRateofSlab { get; set; }

		public bool SendeTicket { get; set; }
		public GuestDetailsRequest GuestInfo { get; set; }

		public DateTime CreatedDate { get; set; }

		public string Address { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string StateCode { get; set; }
		public string Country { get; set; }
		public string CountryCode { get; set; }

		public string ZipCode { get; set; }

		[JsonIgnore]
		public bool IsGuestAddRequired { get; set; }


		[JsonIgnore]
		public DateTime CurrentDate { get; set; }

		public string Currency { get; set; }

		public string Symbol { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsFromQRScan { get; set; }
		public string Mobile { get; set; }
		public string Email { get; set; }
		public string LogoUrl { get; set; }
		public bool IsPayLaterModeFromAdmin { get; set; }
		public decimal? ConvenienceFee { get; set; }
		public int? BookingCategoryId { get; set; }
		public string Notes { get; set; }
		public string PaypalCustomerId { get; set; }
		public string TransactionId { get; set; }
		
		public long? GateSettingId {get; set;}
	}

    public class BookingFromQrRequest
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }

        public string PaymentGatewayCustomerId { get; set; }
        public long? CustomerVehicleId { get; set; }
        public long ParkingLocationId { get; set; }

        [Required]
        public string BookingType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [ValidateDateTime]
        public string StartTime { get; set; }
        [Required]
        [ValidateDateTime]
        public string EndTime { get; set; }
        [Required]
        public string TimeZoneId { get; set; }
        public string TimeZone { get; set; }

        public bool IsFullTimeBooking { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PaymentMode { get; set; }
        public long? UserId { get; set; }
        public bool IsEarlyBirdOfferApplied { get; set; }
        public bool IsNightFareOfferApplied { get; set; }
        public long? EarlyBirdId { get; set; }
        public long? NightFareId { get; set; }
        public decimal? EarlyBirdAmount { get; set; }
        public decimal? EarlyBirdTaxAmount { get; set; }
        public decimal? EarlyBirdFinalAmount { get; set; }
        public decimal? NightFareAmount { get; set; }
        public decimal? NightFareTaxAmount { get; set; }
        public decimal? NightFareFinalAmount { get; set; }
        public bool IsPaymentFromCustomerSite { get; set; }
        public StripeModel StripeInfo { get; set; }

        public SquareupModel SquareupInfo { get; set; }
        public PaypalPaymentModel PaypalInfo { get; set; }
        public decimal PerHourRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? OverSizedCharges { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public double? MaxDurationofSlab { get; set; }
        public decimal? MaxRateofSlab { get; set; }

        public bool SendeTicket { get; set; }
        public GuestDetailsRequest GuestInfo { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

        public string ZipCode { get; set; }

        [JsonIgnore]
        public bool IsCustomerAddRequired { get; set; }


        [JsonIgnore]
        public DateTime CurrentDate { get; set; }

        public string Currency { get; set; }

        public string Symbol { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsFromQRScan { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public bool IsPayLaterModeFromAdmin { get; set; }
        public decimal? ConvenienceFee { get; set; }
        public int? BookingCategoryId { get; set; }
        public string Notes { get; set; }
        public string PaypalCustomerId { get; set; }
        public string TransactionId { get; set; }

        public long? GateSettingId { get; set; }

        public decimal? TaxPercent { get; set; }
    }
}