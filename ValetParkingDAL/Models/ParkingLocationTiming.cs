using System;

namespace ValetParkingDAL.Models
{
    public class ParkingLocationTiming
    {
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StartDateUtc { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan StartTimeUtc { get; set; }
        public TimeSpan EndTimeUtc { get; set; }

    }
}