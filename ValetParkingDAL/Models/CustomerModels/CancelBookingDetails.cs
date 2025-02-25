namespace ValetParkingDAL.Models.CustomerModels
{
    public class CancelBookingDetails
    {
        public string PaymentProvider { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string TransactionId { get; set; }
        public string SecretKey { get; set; }
        public string AccessToken { get; set; }

        public bool IsProduction { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string LocationId { get; set; }
        public string ApiKey { get; set; }

    }
}