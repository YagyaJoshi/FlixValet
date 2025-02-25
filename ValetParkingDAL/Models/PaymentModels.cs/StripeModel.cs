namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class StripeModel
    {
        public string Token { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }

        public bool IsProduction { get; set; }

        public string BusinessTitle { get; set; }
    }
}