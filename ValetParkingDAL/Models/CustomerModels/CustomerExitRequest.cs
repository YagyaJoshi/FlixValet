using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace ValetParkingDAL.Models.CustomerModels
{
	public class CustomerExitRequest
	{
		[Required]
		public long EnterExitId { get; set; }
		[Required]
		public string ExitTime { get; set; }
		[Required]
		public DateTime ExitDate { get; set; }
		public string Notes { get; set; }
		public long BookingId { get; set; }
		// public decimal? TaxAmount { get; set; }
		public decimal TaxPercent { get; set; }
		public decimal BookingAmount { get; set; }
		// public decimal? NetAmount { get; set; }
		public string PaymentMode { get; set; }
		public decimal Duration { get; set; }
		public DateTime? LastBookingDate { get; set; }
		public string BookingType { get; set; }
		public string TimeZone { get; set; }
		public decimal OverweightCharges { get; set; }
		public decimal ConvenienceFee { get; set; }
		public bool IsEarlyBirdOfferApplied { get; set; }
		public decimal TotalOverStayDuration { get; set; }
		public decimal TotalOverStayCharges { get; set; }
		public decimal UnpaidAmount { get; set; }
		public decimal PaidAmount { get; set; }
		public long ParkingLocationId { get; set; }
		public long BookingCategoryId { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public string EntryDate { get; set; }

		public string EnterTime { get; set; }

		public double MaxDurationofSlab { get; set; }

		public decimal MaxRateofSlab { get; set; }
		public string Currency { get; set; }
		public string Mobile { get; set; }
		public long UserId { get; set; }
		
		public long? GateSettingId { get; set; }
	}
}