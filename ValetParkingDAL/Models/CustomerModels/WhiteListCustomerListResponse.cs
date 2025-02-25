using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{

    public class WhiteListCustomerListResponse
    {
        public List<WhiteListCustomerList> WhiteListCustomers { get; set; }
        public int Total { get; set; }
    }
    public class WhiteListCustomerList
    {
        public long WhiteListCustomerId { get; set; }

        public string NumberPlate { get; set; }
    }
}