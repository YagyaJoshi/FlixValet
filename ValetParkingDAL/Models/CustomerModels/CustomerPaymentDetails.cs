using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CustomerPaymentDetails
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Currency { get; set; }
        public string ZipCode { get; set; }
        public string Mobile { get; set; }

        public string PaypalCustomerId { get; set; }

    }
}