namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class SquareupModel
    {
        public string SourceId { get; set; }
        public string AccessToken { get; set; }
        public string ApplicationId { get; set; }
        public string LocationId { get; set; }
        public bool IsProduction { get; set; }

        public string BusinessTitle { get; set; }

        public string IsMonthlySubscription { get; set; }
        public string PricingPlanId { get; set; }
    }
}