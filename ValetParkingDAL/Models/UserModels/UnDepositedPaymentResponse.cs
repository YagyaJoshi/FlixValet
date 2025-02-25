using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.UserModels
{
    public class UnDepositedPaymentResponse
    {
        public List<UnDepositedPaymentList> UndepositedList { get; set; }
        public int Total { get; set; }
        public decimal TotalAmount { get; set; }
        [JsonIgnore]
        public decimal? LastDepositedAmount { get; set; }
        [JsonIgnore]
        public DateTime? LastDepositedDate { get; set; }

        [JsonIgnore]
        public string Symbol { get; set; }

        public string LastDepositedMessage { get; set; }
        public long PaymentId { get; set; }
    }
    public class UnDepositedPaymentList
    {
        public long PaymentId { get; set; }
        public string NumberPlate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}