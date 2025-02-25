namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalErrorResponse
    {
        public object CreditCardVerification { get; set; }
        public object Transaction { get; set; }
        public object Plan { get; set; }
        public object Subscription { get; set; }
        public Errors Errors { get; set; }
        public Parameters Parameters { get; set; }
        public string Message { get; set; }
        public object Target { get; set; }
    }
    public class Errors
    {
        public int Count { get; set; }
        public int DeepCount { get; set; }
    }
    public class Parameters
    {
        public string TransactionAmount { get; set; }
        public string TransactionPaymentMethodNonce { get; set; }
        public string TransactionType { get; set; }
        public string TransactionOptionsSubmitForSettlement { get; set; }
    }



}