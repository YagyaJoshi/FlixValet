namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class ClientTokenResponse
    {
        public string PaypalCustomerId { get; set; }
        public string ClientToken { get; set; }
    }
}