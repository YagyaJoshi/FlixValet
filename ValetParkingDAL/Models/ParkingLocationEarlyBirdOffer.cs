namespace ValetParkingDAL.Models
{
    public class ParkingLocationEarlyBirdOffer
    {
        public long Id { get; set; }
        public long ParkingLocationId { get; set; }
        public string BookingType { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }
        public string EnterFromTime { get; set; }
        public string EnterToTime { get; set; }
        public string ExitByTime { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }

    }
}