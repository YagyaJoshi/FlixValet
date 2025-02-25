using System.Collections.Generic;

namespace ValetParkingDAL.Models
{
	public class ParkingLocationGateSettingResponse
	{
		public List<LocationGateSettingList> ParkingLocationsGateSettings { get; set; }
		public int Total { get; set; }
	}
	
	public class LocationGateSettingList
	{
		public long Id { get; set; }
		public string LocationName { get; set; }
		public long ParkingLocationId { get; set; }

		public string RelayURL { get; set; }

		public string QueueName { get; set; }
		public string QueueURL { get; set; }
		public string RelayName { get; set; }
		
		public string QRCodePath {get; set;}
	}
}