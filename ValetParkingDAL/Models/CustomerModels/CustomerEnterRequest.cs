using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
	public class CustomerEnterRequest
	{

		public long Id { get; set; }
		[Required]
		public long ParkingLocationId { get; set; }
		[Required]
		public long CustomerBookingId { get; set; }
		public long? CustomerInfoId { get; set; }
		public long? CustomerVehicleId { get; set; }
		[Required]
		public string EntryDate { get; set; }
		[Required]
		public string EnterTime { get; set; }
		public string Notes { get; set; }

		public long? GateSettingId {get; set;}
	}
}