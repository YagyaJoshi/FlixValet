namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class SquareupPaymentRequest
    {
        public string source_id { get; set; }
        public string idempotency_key { get; set; }
        public AmountModel amount_money { get; set; }
        public BillingAddress billing_address { get; set; }

    }

    public class AmountModel
    {
        public long amount { get; set; }
        public string currency { get; set; }
    }

    public class BillingAddress
    {
        public string address_line_1 { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }

        /*Corresponds to city*/
        public string locality { get; set; }

        /*Corresponds to state*/
        public string administrative_district_level_1 { get; set; }

        /*Corresponds to country*/
        public string administrative_district_level_2 { get; set; }
        public string postal_code { get; set; }
    }
}