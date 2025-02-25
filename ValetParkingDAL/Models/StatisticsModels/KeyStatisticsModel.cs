using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class KeyStatisticsModel
    {
        public List<LocationReport> StatisticsReport { get; set; }

    }


    public class LocationReport
    {
        public long? ParkingLocationId { get; set; }
        public string LocationName { get; set; }
        public bool IsSummarizedReport { get; set; }
        public LocationStatistics<decimal> Revenue { get; set; }

        public LocationStatistics<long> Transactions { get; set; }
        public LocationStatistics<long> PeakOccupancy { get; set; }
        public LocationStatistics<decimal> RevenuePerTransaction { get; set; }
    }


    public class LocationStatistics<T>
    {
        public T LastYearReport { get; set; }
        public T ThisYearReport { get; set; }
        public T Variation { get; set; }
        public decimal VariationPercentage { get; set; }

    }

    #region 
    // public long ThisYearTransactions { get; set; }

    // public long LastYearTransactions { get; set; }



    // public decimal TransactionsPrcntHike { get; set; }

    // public long ThisYearRevenue { get; set; }
    // public long LastYearRevenue { get; set; }
    // public decimal RevenuePrcntHike { get; set; }
    // public long ThisYearPeakOccupancy { get; set; }
    // public long LastYearPeakOccupancy { get; set; }
    // public long ThisYearRevenuePerTrans { get; set; }
    // public long LastYearRevenuePerTrans { get; set; }


    #endregion
}