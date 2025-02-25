namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalModel
    {
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool IsProduction { get; set; }
        // public long UserId { get; set; }
        public long CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PaypalCustomerId { get; set; }

    }
}