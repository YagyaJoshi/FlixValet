using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
	public class ParkingLocationGateSettings
	{
		public long Id { get; set; }

		[Required]
		public long ParkingLocationId { get; set; }

		[Required]
		public long RelayTypeId { get; set; }

		[Required]
		public string RelayURL { get; set; }
		public string QueueName { get; set; }
		public string QueueURL { get; set; }
		public string Region { get; set; }
		public string RelayName { get; set;}
		public bool IsEnter {get; set;}
		
		public long GateNumber { get; set;}
		
		public string QRCodePath {get; set;}
	}
}