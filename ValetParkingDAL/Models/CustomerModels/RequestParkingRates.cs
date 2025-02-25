using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class RequestParkingRates
    {

        [JsonIgnore]
        public long ParkingLocationId { get; set; }
        public string BookingType { get; set; }
        public int Duration { get; set; }
        public decimal Charges { get; set; }
    }
}