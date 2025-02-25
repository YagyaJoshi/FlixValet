namespace ValetParkingDAL.Models
{
    public class ChargeBackReportInput
    {
        public long ParkingBusinessOwnerId { get; set; }

        public long BusinessOfficeId { get; set; }
        public string Url { get; set; }
    }
}
