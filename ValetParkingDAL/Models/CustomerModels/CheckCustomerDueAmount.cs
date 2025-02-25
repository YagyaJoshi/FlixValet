namespace ValetParkingDAL.Models.CustomerModels
{
    public class CheckCustomerDueAmount
    {
        public decimal TotalCharges { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal UnpaidAmount { get; set; }

        public string TimeZoneId { get; set; }
    }
}