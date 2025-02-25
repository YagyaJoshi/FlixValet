using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class AdditionalPaymentRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "Please enter valid Booking Id.")]
        public long BookingId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please enter valid User Id.")]
        public long UserId { get; set; }

        public DateTime ExitDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }
        public string EntryDate { get; set; }
        public string EnterTime { get; set; }
        public decimal TotalOverStayDuration { get; set; }
        public decimal TotalOverStayCharges { get; set; }
        public decimal UnpaidAmount { get; set; }

        [Required]
        public string PaymentMode { get; set; }
        [Required]
        public string Currency { get; set; }
        public double? MaxDurationofSlab { get; set; }

        public decimal? MaxRateofSlab { get; set; }

        public string Mobile { get; set; }

        public string TransactionId { get; set; }

        public decimal? ConvenienceFee { get; set; }

    }
}