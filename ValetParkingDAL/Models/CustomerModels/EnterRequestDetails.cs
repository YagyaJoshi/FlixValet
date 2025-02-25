namespace ValetParkingDAL.Models.CustomerModels
{
    public class EnterRequestDetails
    {
        public long EnterId { get; set; }
        public string Mobile { get; set; }
        public int BookingTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public bool SendeTicket { get; set; }

    }
}