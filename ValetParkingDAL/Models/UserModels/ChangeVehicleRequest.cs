namespace ValetParkingDAL.Models.UserModels
{
    public class ChangeVehicleRequest
    {
        public long CustomerBookingId { get; set; }
        public long CustomerVehicleId { get; set; }
    }
}