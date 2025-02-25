using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class OverStayRef
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime StartDateUtc { get; set; }

        public DateTime EndDateUtc { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal OverStayDuration { get; set; }

        public decimal OverStayCharges { get; set; }
    }
}