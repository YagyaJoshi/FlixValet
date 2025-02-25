using ValetParkingDAL.Models.StatisticsModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IStatistics
    {
        KeyStatisticsModel KeyStatistics(long ParkingBusinessOwnerId, string CurrentDate);
        KeyStatistics_PieGraphModel KeyStatistics_PieGraphs(long ParkingLocationId, string CurrentDate, string Filter);
        GraphResponseModel RevenueGraph(long ParkingLocationId, string CurrentDate, string Filter);
        GraphResponseModel TransactionsGraph(long ParkingLocationId, string CurrentDate, string Filter);
        DurationGraphModel DurationsGraph(long ParkingLocationId, string CurrentDate);
        DurationGraphModel DurationsGraphv1(long ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize);
        OccupancyGridGraphModel OccupancyGraph(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize);

        OccupancyGridGraphModel OccupancyGraphv1(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize);

        LiveReportModel LiveReport(long ParkingLocationId, string CurrentDate);

        LiveReportV1Model LiveReport_v1(long ParkingLocationId, string CurrentDate);

        GraphResponseModel RevenueGraph_v1(long ParkingLocationId, string CurrentDate, string Filter);
        GraphResponseModel TransactionsGraph_v1(long ParkingLocationId, string CurrentDate, string Filter);

        AccountReconcilationModel AccountReconcilation(long ParkingLocationId, string CurrentDate);

        AccountReconcilationModel AccountReconcilation_v1(long ParkingLocationId, string CurrentDate, string depositsortColumn, string depositsortOrder, int? depositpageNo, int? depositpageSize, string bookingsortColumn, string bookingsortOrder, int? bookingpageNo, int? bookingpageSize);

    }
}