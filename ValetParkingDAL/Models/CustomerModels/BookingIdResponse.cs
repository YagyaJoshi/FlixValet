namespace ValetParkingDAL.Models.CustomerModels
{
    public class BookingIdResponse
    {
        public long? BookingId { get; set; }
        public long? WhiteListCustomerId { get; set; }
        //  public long? ChargeBackCustomerId { get; set; }
        public BusinessOfficeEmployeeDetails ChargeBackCustomerDetails { get; set; }
    }
}