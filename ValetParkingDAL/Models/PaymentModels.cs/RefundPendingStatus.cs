namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class RefundPendingStatus
    {
        public string RefundId { get; set; }
        public string RefundStatus { get; set; }
        public long CustomerBookingId { get; set; }
        public string PaymentProvider { get; set; }
    }
}