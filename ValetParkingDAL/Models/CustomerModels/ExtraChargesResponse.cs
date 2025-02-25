namespace ValetParkingDAL.Models.CustomerModels
{
    public class ExtraChargesResponse
    {
        public long BookingId { get; set; }
        public bool IsExtraChargesApplied { get; set; }
        public decimal BookingCharges { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingMessage { get; set; }
    }
}