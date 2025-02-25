using ValetParkingDAL.Models.CustomerModels;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class BookingPaymentDetails
    {
        public BookingDetailResponse BookingDetails { get; set; }
        public ParkingLocDetailsResponse ParkingLocationDetails { get; set; }

        public decimal? PaidAmount { get; set; }
        public decimal BookingAmount { get; set; }
        public decimal DueAmount { get; set; }
    }
}