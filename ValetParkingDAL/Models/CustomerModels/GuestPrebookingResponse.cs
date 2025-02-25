using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestPrebookingResponse
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PerHourRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OverSizedCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string MaxStay { get; set; }
        public string BookingMessage { get; set; }
        public string Symbol { get; set; }
        public CustomerVehicleResponse CustomerVehicle { get; set; }
        public bool IsEarlyBirdOfferApplicable { get; set; }
        public bool IsNightFareOfferApplicable { get; set; }

        public EarlyBirdInfo EarlyBirdInfo { get; set; }
        public NightFareInfo NightFareInfo { get; set; }

        public decimal? EarlyBirdTaxAmount { get; set; }
        public decimal? EarlyBirdFinalAmount { get; set; }

        public decimal? NightFareTaxAmount { get; set; }
        public decimal? NightFareFinalAmount { get; set; }
        public double MaxDurationofSlab { get; set; }

        public decimal MaxRateofSlab { get; set; }
        public bool HasPaymentSetup { get; set; }
        public decimal ConvenienceFee { get; set; }
        public bool IsWhiteListCustomer { get; set; }
        public bool IsChargeBackCustomer { get; set; }
        public string LocationId { get; set; }
        public string AccessToken { get; set; }
        public decimal TaxAmountWithConvenienceFee { get; set; }
        public decimal FinalAmountWithConvenienceFee { get; set; }

        public decimal? EarlyBirdTaxAmountWithConvenienceFee { get; set; }
        public decimal? EarlyBirdFinalAmountWithConvenienceFee { get; set; }

        public decimal? NightFareTaxAmountWithConvenienceFee { get; set; }
        public decimal? NightFareFinalAmountWithConvenienceFee { get; set; }
    }
}