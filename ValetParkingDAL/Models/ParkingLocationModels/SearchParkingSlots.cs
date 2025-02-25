using System;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class SearchParkingSlots
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan StartTimeUtc { get; set; }
        public TimeSpan EndTimeUtc { get; set; }
        public string WeekDay { get; set; }
        public Boolean IsMonday { get; set; }
        public Boolean IsTuesday { get; set; }
        public Boolean IsWednesday { get; set; }
        public Boolean IsThursday { get; set; }
        public Boolean IsFriday { get; set; }
        public Boolean IsSaturday { get; set; }
        public Boolean IsSunday { get; set; }

    }
}