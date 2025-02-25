namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalInfo
    {
        public string ClientToken { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool IsProduction { get; set; }
    }
}