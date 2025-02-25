namespace ValetParkingDAL.Models.CustomerModels
{
    public class EarlyBirdInfo
    {
        public long EarlyBirdId {get;set;}
        public string EnterFromTime { get; set; }
        public string EnterToTime { get; set; }
        public string ExitByTime { get; set; }
        public decimal Amount { get; set; }
    }
    public class NightFareInfo
    {
        public long NightFareId { get; set; }
        public string EnterFromTime { get; set; }
        public string EnterToTime { get; set; }
        public string ExitByTime { get; set; }
        public decimal Amount { get; set; }
    }

}