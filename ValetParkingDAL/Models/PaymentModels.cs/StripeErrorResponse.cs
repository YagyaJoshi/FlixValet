namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class StripeErrorResponse
    {
        public Error error { get; set; }

    }
    public class Error
    {
        public string code { get; set; }
        public string doc_url { get; set; }
        public string message { get; set; }
        public string param { get; set; }
        public string type { get; set; }
    }


}