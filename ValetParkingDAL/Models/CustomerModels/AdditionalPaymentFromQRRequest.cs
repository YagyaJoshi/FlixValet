using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingDAL.Models.CustomerModels
{
	public class AdditionalPaymentFromQRRequest
	{
		public long BookingId { get; set; }
		public long CustomerId { get; set; }
		public long ParkingLocationId { get; set; }
		public decimal UnpaidAmount { get; set; }
		public string PaymentMethod { get; set; }
		public string Currency { get; set; }
		public string PaymentNotes { get; set; }
		public string ZipCode { get; set; }
		public decimal ConvenienceFee { get; set; }
		public decimal TotalCharges { get; set; }
		public StripeModel StripeInfo { get; set; }
		public SquareupModel SquareupInfo { get; set; }
		public PaypalPaymentModel PaypalInfo { get; set; }


		[JsonIgnore]
		public string TimeZone { get; set; }

		[JsonIgnore]
		public DateTime CurrentDate { get; set; }

		public string PaypalCustomerId { get; set; }
		public decimal TaxAmount { get; set; }
		
		public decimal BookingAmount { get; set; }
		
		public List<BookingDetails> BookingDetails { get; set; }

	}
}