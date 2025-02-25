namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalPaymentModel
    {

        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool IsProduction { get; set; }
        public string Nonce { get; set; }
    }
}