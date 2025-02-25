using System;

namespace ValetParkingDAL.Models.PaymentModels.cs
{

    public class SquareupChargesApiResponse
    {
        public Payment payment { get; set; }
    }

    public class AmountMoney
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class SquareupCard
    {
        public string card_brand { get; set; }
        public string last_4 { get; set; }
        public int exp_month { get; set; }
        public int exp_year { get; set; }
        public string fingerprint { get; set; }
        public string card_type { get; set; }
        public string prepaid_type { get; set; }
        public string bin { get; set; }
    }

    public class CardPaymentTimeline
    {
        public DateTime authorized_at { get; set; }
        public DateTime captured_at { get; set; }
    }

    public class CardDetails
    {
        public string status { get; set; }
        public SquareupCard card { get; set; }
        public string entry_method { get; set; }
        public string cvv_status { get; set; }
        public string avs_status { get; set; }
        public string statement_description { get; set; }
        public CardPaymentTimeline card_payment_timeline { get; set; }
    }

    public class RiskEvaluation
    {
        public DateTime created_at { get; set; }
        public string risk_level { get; set; }
    }


    public class TotalMoney
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class ApprovedMoney
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Payment
    {
        public string id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public AmountMoney amount_money { get; set; }
        public string status { get; set; }
        public string delay_duration { get; set; }
        public string source_type { get; set; }
        public CardDetails card_details { get; set; }
        public string location_id { get; set; }
        public string order_id { get; set; }
        public RiskEvaluation risk_evaluation { get; set; }
        public BillingAddress billing_address { get; set; }
        public TotalMoney total_money { get; set; }
        public ApprovedMoney approved_money { get; set; }
        public string receipt_number { get; set; }
        public string receipt_url { get; set; }
        public string delay_action { get; set; }
        public DateTime delayed_until { get; set; }
        public string version_token { get; set; }

        public string idempotency_key { get; set; }
    }



}