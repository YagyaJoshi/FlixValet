using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Helpers;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models.StatisticsModels;

namespace ValetParkingBLL.Repository
{
    public class StatisticsRepo : IStatistics
    {
        private readonly IConfiguration _configuration;
        private readonly StatisticsHelper _statisticsHelper;
        private readonly IMapper _mapper;

        public StatisticsRepo(IConfiguration configuration, StatisticsHelper statisticsHelper, IMapper mapper)
        {
            _configuration = configuration;
            _statisticsHelper = statisticsHelper;
            _mapper = mapper;
            var config = new MapperConfiguration(cfg =>
      {
          cfg.CreateMap<LiveReportDbModel, LiveReportModel>();

      });
            _mapper = config.CreateMapper();
        }

        public AccountReconcilationModel AccountReconcilation(long ParkingLocationId, string CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AccountReconcilation_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);


                DataSet ds = objSQL.FetchDB(objCmd);

                var reconcilationReport = (from DataRow dr in ds.Tables[0].Rows
                                           select new AccountReconcilationModel
                                           {
                                               CashCollected = Convert.ToDecimal(dr["CashCollected"]),
                                               CardCollected = Convert.ToDecimal(dr["CardCollected"]),
                                               TicketsIssued = Convert.ToInt32(dr["TicketsIssued"]),
                                               OpenTickets = Convert.ToInt32(dr["OpenTickets"])
                                           }).FirstOrDefault();
                if (reconcilationReport != null)
                {

                    reconcilationReport.DepositReport = (from DataRow dr in ds.Tables[1].Rows
                                                         select new DepositReport
                                                         {
                                                             Depositor = Convert.ToString(dr["Depositor"]),
                                                             DepositedAmount = Convert.ToDecimal(dr["DepositedAmount"]),
                                                             Source = Convert.ToString(dr["Source"])
                                                         }).ToList();


                    reconcilationReport.BookingDataList = (from DataRow dr in ds.Tables[2].Rows
                                                           select new BookingDataModel
                                                           {
                                                               NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                                               Amount = Convert.ToDecimal(dr["Amount"]),
                                                               CustomerName = string.IsNullOrEmpty(Convert.ToString(dr["CustomerName"]).Trim()) ? "Guest" : Convert.ToString(dr["CustomerName"]),
                                                               Source = Convert.ToString(dr["Source"])
                                                           }).ToList();
                    reconcilationReport.Summary = new Summary
                    {
                        CardRevenue = reconcilationReport.BookingDataList.Where(a => a.Source.ToLower().Equals("credit card")).Sum(a => a.Amount),
                        CashRevenue = reconcilationReport.BookingDataList.Where(a => a.Source.ToLower().Equals("cash")).Sum(a => a.Amount)
                    };
                    if (reconcilationReport.Summary != null)
                    {
                        reconcilationReport.Summary.Deficit = reconcilationReport.Summary.CashRevenue - (reconcilationReport.DepositReport.Sum(a => a.DepositedAmount));
                        reconcilationReport.Summary.Total = reconcilationReport.Summary.CashRevenue + reconcilationReport.Summary.CardRevenue;
                    }

                    return reconcilationReport;
                }
                else
                    return new AccountReconcilationModel();
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }

        }

        public AccountReconcilationModel AccountReconcilation_v1(long ParkingLocationId, string CurrentDate, string depositsortColumn, string depositsortOrder, int? depositpageNo, int? depositpageSize, string bookingsortColumn, string bookingsortOrder, int? bookingpageNo, int? bookingpageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AccountReconcilation_v4");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@depositpageNo", depositpageNo);
                objCmd.Parameters.AddWithValue("@depositpageSize", depositpageSize);
                objCmd.Parameters.AddWithValue("@depositsortColumn", depositsortColumn);
                objCmd.Parameters.AddWithValue("@depositsortOrder", depositsortOrder);
                objCmd.Parameters.AddWithValue("@bookingsortColumn", bookingsortColumn);
                objCmd.Parameters.AddWithValue("@bookingsortOrder", bookingsortOrder);
                objCmd.Parameters.AddWithValue("@bookingpageNo", bookingpageNo);
                objCmd.Parameters.AddWithValue("@bookingpageSize", bookingpageSize);

                DataSet ds = objSQL.FetchDB(objCmd);

                var reconcilationReport = (from DataRow dr in ds.Tables[0].Rows
                                           select new AccountReconcilationModel
                                           {
                                               CashCollected = Convert.ToDecimal(dr["CashCollected"]),
                                               CardCollected = Convert.ToDecimal(dr["CardCollected"]),
                                               TicketsIssued = Convert.ToInt32(dr["TicketsIssued"]),
                                               OpenTickets = Convert.ToInt32(dr["OpenTickets"])
                                           }).FirstOrDefault();
                if (reconcilationReport != null)
                {

                    reconcilationReport.DepositReport = (from DataRow dr in ds.Tables[2].Rows
                                                         select new DepositReport
                                                         {
                                                             Depositor = Convert.ToString(dr["Depositor"]),
                                                             DepositedAmount = Convert.ToDecimal(dr["DepositedAmount"]),
                                                             Source = Convert.ToString(dr["Source"])
                                                         }).ToList();


                    reconcilationReport.BookingDataList = (from DataRow dr in ds.Tables[3].Rows
                                                           select new BookingDataModel
                                                           {
                                                               NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                                               Amount = Convert.ToDecimal(dr["Amount"]),
                                                               CustomerName = string.IsNullOrEmpty(Convert.ToString(dr["CustomerName"]).Trim()) ? "Guest" : Convert.ToString(dr["CustomerName"]),
                                                               Source = Convert.ToString(dr["Source"]),

                                                           }).ToList();
                    reconcilationReport.Summary = ds.Tables[3].Rows.Count > 0 ? new Summary
                    {

                        CashRevenue = ds.Tables[3].Rows[0]["CashRevenue"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[3].Rows[0]["CashRevenue"]) : 0,
                        CardRevenue = ds.Tables[3].Rows[0]["CardRevenue"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[3].Rows[0]["CardRevenue"]) : 0
                        // CardRevenue = reconcilationReport.BookingDataList.Where(a => a.Source.ToLower().Equals("credit card")).Sum(a => a.Amount),
                        // CashRevenue = reconcilationReport.BookingDataList.Where(a => a.Source.ToLower().Equals("cash")).Sum(a => a.Amount)
                    } : null;
                    if (reconcilationReport.Summary != null)
                    {
                        reconcilationReport.Summary.Deficit = ds.Tables[1].Rows[0]["Deficit"] == DBNull.Value ? 0 : Convert.ToInt32(ds.Tables[1].Rows[0]["Deficit"]);
                        //reconcilationReport.Summary.Deficit = reconcilationReport.Summary.CashRevenue - (reconcilationReport.DepositReport.Sum(a => a.DepositedAmount));
                        reconcilationReport.Summary.Total = reconcilationReport.Summary.CashRevenue + reconcilationReport.Summary.CardRevenue;
                    }
                    if (reconcilationReport.DepositReport != null && reconcilationReport.DepositReport.Count > 0)
                        reconcilationReport.DepositTotal = Convert.ToInt32(ds.Tables[2].Rows[0]["TotalCount"]);

                    if (reconcilationReport.BookingDataList != null && reconcilationReport.BookingDataList.Count > 0)
                        reconcilationReport.BookingDataTotal = Convert.ToInt32(ds.Tables[3].Rows[0]["TotalCount"]);

                    return reconcilationReport;
                }
                else
                    return new AccountReconcilationModel();
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public DurationGraphModel DurationsGraph(long ParkingLocationId, string CurrentDate)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DurationGraph_v2");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataSet ds = objSQL.FetchDB(objCmd);

                var durationReport = (from DataRow dr in ds.Tables[0].Rows
                                      select new DurationReport
                                      {
                                          BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                          Interval = Convert.ToInt32(dr["Duration"]),
                                          BookingsCount = Convert.ToInt64(dr["BookingsCount"])
                                      }).ToList();


                var bookingList = (from DataRow dr in ds.Tables[1].Rows
                                   select new BookingListModel
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       StartDate = Convert.ToDateTime(dr["StartDate"]),
                                       EndDate = Convert.ToDateTime(dr["EndDate"]),
                                       Duration = _statisticsHelper.GetDateDifference(Convert.ToDateTime(dr["StartDate"]), Convert.ToDateTime(dr["EndDate"]), Convert.ToInt32(dr["BookingTypeId"]) == 1 ? false : true),
                                       NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                                       NumberPlate = Convert.ToString(dr["NumberPlate"])
                                   }).ToList();

                int MaxDuration = _statisticsHelper.FindNextBestEvenValue(durationReport.Max(a => a.Interval));
                return new DurationGraphModel { BookingList = bookingList, DurationReport = _statisticsHelper.GetGraphResponse(durationReport, "DurationGraph", CurrentDate, MaxDuration) };
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public DurationGraphModel DurationsGraphv1(long ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DurationGraph_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@pageNo", pageNo);
                objCmd.Parameters.AddWithValue("@pageSize", pageSize);
                objCmd.Parameters.AddWithValue("@sortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@sortOrder", sortOrder);

                DataSet ds = objSQL.FetchDB(objCmd);

                var durationReport = (from DataRow dr in ds.Tables[0].Rows
                                      select new DurationReport
                                      {
                                          BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                          Interval = Convert.ToInt32(dr["Duration"]),
                                          BookingsCount = Convert.ToInt64(dr["BookingsCount"])
                                      }).ToList();


                var bookingList = (from DataRow dr in ds.Tables[1].Rows
                                   select new BookingListModel
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       StartDate = Convert.ToDateTime(dr["StartDate"]),
                                       EndDate = Convert.ToDateTime(dr["EndDate"]),
                                       Duration = _statisticsHelper.GetDateDifference(Convert.ToDateTime(dr["StartDate"]), Convert.ToDateTime(dr["EndDate"]), Convert.ToInt32(dr["BookingTypeId"]) == 1 ? false : true),
                                       NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                                       NumberPlate = Convert.ToString(dr["NumberPlate"])
                                   }).ToList();

                int MaxDuration = durationReport.Count > 0 ? _statisticsHelper.FindNextBestEvenValue(durationReport.Max(a => a.Interval)) : 0;
                return new DurationGraphModel { BookingList = bookingList, DurationReport = _statisticsHelper.GetGraphResponse(durationReport, "DurationGraph", CurrentDate, MaxDuration), Total = bookingList.Count > 0 ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalCount"]) : 0 };
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public KeyStatisticsModel KeyStatistics(long ParkingBusinessOwnerId, string CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_KeyStatistics");
            try
            {
                List<LocationReport> lstReport = new List<LocationReport>();
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataTable dtStatistics = objSQL.FetchDT(objCmd);
                LocationReport locationReport;
                foreach (DataRow dr in dtStatistics.Rows)
                {
                    locationReport = new LocationReport();

                    locationReport.ParkingLocationId = (dr["ParkingLocationId"]) == DBNull.Value ? (long?)null : Convert.ToInt64(dr["ParkingLocationId"]);
                    locationReport.LocationName = Convert.ToString(dr["LocationName"]);

                    locationReport.Revenue = new LocationStatistics<decimal>
                    {
                        ThisYearReport = Convert.ToDecimal(dr["ThisYearRevenue"]),
                        LastYearReport = Convert.ToDecimal(dr["LastYearRevenue"]),
                        Variation = Convert.ToDecimal(dr["ThisYearRevenue"]) - Convert.ToDecimal(dr["LastYearRevenue"]),
                        VariationPercentage = _statisticsHelper.calculatePercentage(Convert.ToDecimal(dr["ThisYearRevenue"]), Convert.ToDecimal(dr["LastYearRevenue"]))
                    };

                    locationReport.Transactions = new LocationStatistics<long>
                    {
                        ThisYearReport = Convert.ToInt64(dr["ThisYearTransactions"]),
                        LastYearReport = Convert.ToInt64(dr["LastYearTransactions"]),
                        Variation = Convert.ToInt64(dr["ThisYearTransactions"]) - Convert.ToInt64(dr["LastYearTransactions"]),
                        VariationPercentage = _statisticsHelper.calculatePercentage(Convert.ToDecimal(dr["ThisYearTransactions"]), Convert.ToDecimal(dr["LastYearTransactions"]))

                    };

                    var CurrentRevenuePerTransaction = locationReport.Transactions.ThisYearReport == 0 ? locationReport.Revenue.ThisYearReport : _statisticsHelper.RoundOff(locationReport.Revenue.ThisYearReport / locationReport.Transactions.ThisYearReport);


                    var LstYrRevenuePerTransaction = locationReport.Transactions.LastYearReport == 0 ? locationReport.Revenue.LastYearReport : _statisticsHelper.RoundOff(locationReport.Revenue.LastYearReport / locationReport.Transactions.LastYearReport);

                    locationReport.RevenuePerTransaction = new LocationStatistics<decimal>
                    {
                        ThisYearReport = CurrentRevenuePerTransaction,
                        LastYearReport = LstYrRevenuePerTransaction,
                        Variation = CurrentRevenuePerTransaction - LstYrRevenuePerTransaction,
                        VariationPercentage = _statisticsHelper.calculatePercentage(CurrentRevenuePerTransaction, LstYrRevenuePerTransaction)
                    };

                    locationReport.PeakOccupancy = new LocationStatistics<long>
                    {
                        ThisYearReport = Convert.ToInt64(dr["ThisYearPeakOccupancy"]),
                        LastYearReport = Convert.ToInt64(dr["LastYearPeakOccupancy"]),
                        Variation = Convert.ToInt64(dr["ThisYearPeakOccupancy"]) - Convert.ToInt64(dr["LastYearPeakOccupancy"]),
                        VariationPercentage = _statisticsHelper.calculatePercentage(Convert.ToInt64(dr["ThisYearPeakOccupancy"]), Convert.ToInt64(dr["LastYearPeakOccupancy"]))

                    };
                    locationReport.IsSummarizedReport = false;

                    lstReport.Add(locationReport);
                }

                #region SummarizedReport
                locationReport = new LocationReport();
                locationReport.IsSummarizedReport = true;
                var revenue = new LocationStatistics<decimal>();
                revenue.ThisYearReport = lstReport.Sum(a => a.Revenue.ThisYearReport);
                revenue.LastYearReport = lstReport.Sum(a => a.Revenue.LastYearReport);
                revenue.Variation = revenue.ThisYearReport - revenue.LastYearReport;
                revenue.VariationPercentage = _statisticsHelper.calculatePercentage(revenue.ThisYearReport, revenue.LastYearReport);

                var transactions = new LocationStatistics<long>();
                transactions.ThisYearReport = lstReport.Sum(a => a.Transactions.ThisYearReport);
                transactions.LastYearReport = lstReport.Sum(a => a.Transactions.LastYearReport);
                transactions.Variation = transactions.ThisYearReport - transactions.LastYearReport;
                transactions.VariationPercentage = _statisticsHelper.calculatePercentage(transactions.ThisYearReport, transactions.LastYearReport);


                var revenuepertrans = new LocationStatistics<decimal>();
                revenuepertrans.ThisYearReport = lstReport.Sum(a => a.RevenuePerTransaction.ThisYearReport);
                revenuepertrans.LastYearReport = lstReport.Sum(a => a.RevenuePerTransaction.LastYearReport);
                revenuepertrans.Variation = revenuepertrans.ThisYearReport - revenuepertrans.LastYearReport;
                revenuepertrans.VariationPercentage = _statisticsHelper.calculatePercentage(revenuepertrans.ThisYearReport, revenuepertrans.LastYearReport);


                var peakoccupancy = new LocationStatistics<long>();

                peakoccupancy.ThisYearReport = lstReport.Sum(a => a.PeakOccupancy.ThisYearReport);
                peakoccupancy.LastYearReport = lstReport.Sum(a => a.PeakOccupancy.LastYearReport);
                peakoccupancy.Variation = peakoccupancy.ThisYearReport - peakoccupancy.LastYearReport;
                peakoccupancy.VariationPercentage = _statisticsHelper.calculatePercentage(peakoccupancy.ThisYearReport, peakoccupancy.LastYearReport);

                locationReport.Revenue = revenue;
                locationReport.Transactions = transactions;
                locationReport.RevenuePerTransaction = revenuepertrans;
                locationReport.PeakOccupancy = peakoccupancy;
                lstReport.Add(locationReport);

                #endregion

                return new KeyStatisticsModel { StatisticsReport = lstReport };
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public KeyStatistics_PieGraphModel KeyStatistics_PieGraphs(long ParkingLocationId, string CurrentDate, string Filter)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_KeyStatistics_PieGraphs_V1");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);

                DataSet ds = objSQL.FetchDB(objCmd);

                // if (ds.Tables[2].Rows.Count > 0)
                // {

                KeyStatistics_PieGraphModel response = new KeyStatistics_PieGraphModel();
                // int No_of_Spaces = Convert.ToInt32(ds.Tables[2].Rows[0][0]);


                // int Days;
                // switch (Filter)
                // {
                //     case "thisweek":
                //         Days = 7;
                //         break;
                //     case "thismonth":
                //         Days = System.DateTime.DaysInMonth(Convert.ToDateTime(CurrentDate).Year, Convert.ToDateTime(CurrentDate).Month);
                //         break;
                //     default:
                //         Days = 1;
                //         break;
                // };
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var bookingTypeReport = (from DataRow dr in ds.Tables[0].Rows
                                             select new BookingTypePieModel
                                             {
                                                 BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                                 Revenue = Convert.ToDecimal(dr["Amount"]),
                                                 BookingPercentage = _statisticsHelper.calculatePieGraphPercentageV1(Convert.ToInt64(dr["BookingCount"]), Convert.ToInt64(dr["TotalCount"]))
                                             }).ToList();
                    response.BookingTypeReport = bookingTypeReport;

                }
                if (ds.Tables[1].Rows.Count > 0)
                {
                    var userTypeReport = (from DataRow dr in ds.Tables[1].Rows
                                          select new UserTypePieModel
                                          {
                                              UserType = Convert.ToString(dr["Name"]),
                                              Revenue = Convert.ToDecimal(dr["Amount"]),
                                              BookingPercentage = _statisticsHelper.calculatePieGraphPercentageV1(Convert.ToInt64(dr["BookingCount"]), Convert.ToInt64(dr["TotalCount"]))
                                          }).ToList();
                    response.UserTypeReport = userTypeReport;
                }
                return response;
                // }

                // else throw new AppException("no record found");
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public LiveReportModel LiveReport(long ParkingLocationId, string CurrentDate)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_LiveReport");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataSet ds = objSQL.FetchDB(objCmd);

                var liveReport = (from DataRow dr in ds.Tables[0].Rows
                                  select new LiveReportDbModel
                                  {
                                      HourlyRevenue = Convert.ToDecimal(dr["HourlyRevenue"]),
                                      MonthlyRevenue = Convert.ToDecimal(dr["MonthlyRevenue"]),
                                      NoofTransactions = Convert.ToInt64(dr["NoofTransactions"]),
                                      NoofVehiclesEntered = Convert.ToInt64(dr["NoofVehiclesEntered"])
                                  }).FirstOrDefault();

                var EntryReport = (from DataRow dr in ds.Tables[1].Rows
                                   select new LiveReport
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       Variation = Convert.ToString(dr["EntryTime"]),
                                       Count = Convert.ToInt32(dr["Count"])

                                   }).ToList();

                var ExitReport = (from DataRow dr in ds.Tables[2].Rows
                                  select new LiveReport
                                  {
                                      BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                      Variation = Convert.ToString(dr["ExitTime"]),
                                      Count = Convert.ToInt32(dr["Count"])
                                  }).ToList();

                liveReport.EntryDbReport = EntryReport;
                liveReport.ExitDbReport = ExitReport;

                return _statisticsHelper.GetLiveReportResponse(liveReport);

            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public LiveReportV1Model LiveReport_v1(long ParkingLocationId, string CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_LiveReport_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataSet ds = objSQL.FetchDB(objCmd);

                var liveReport = (from DataRow dr in ds.Tables[0].Rows
                                  select new LiveReportDbModel
                                  {
                                      HourlyRevenue = Convert.ToDecimal(dr["HourlyRevenue"]),
                                      MonthlyRevenue = Convert.ToDecimal(dr["MonthlyRevenue"]),
                                      NoofTransactions = Convert.ToInt64(dr["NoofTransactions"]),
                                      NoofVehiclesEntered = Convert.ToInt64(dr["NoofVehiclesEntered"])
                                  }).FirstOrDefault();

                var EntryReport = (from DataRow dr in ds.Tables[1].Rows
                                   select new LiveReport
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       Variation = Convert.ToString(dr["EntryTime"]),
                                       Count = Convert.ToInt32(dr["Count"])

                                   }).ToList();

                var ExitReport = (from DataRow dr in ds.Tables[2].Rows
                                  select new LiveReport
                                  {
                                      BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                      Variation = Convert.ToString(dr["ExitTime"]),
                                      Count = Convert.ToInt32(dr["Count"])
                                  }).ToList();

                liveReport.EntryDbReport = EntryReport;
                liveReport.ExitDbReport = ExitReport;
                LiveReportV1Model response = new LiveReportV1Model
                {
                    EnterReport = liveReport.EntryDbReport.Count > 0 ? _statisticsHelper.GetGraphResponse_v1(liveReport.EntryDbReport, "daily", CurrentDate) : null,
                    MaxValueEnterReport = liveReport.EntryDbReport.Count > 0 ? _statisticsHelper.FindNextBestValueinTens(liveReport.EntryDbReport.Max(a => a.Count)) : 0,
                    MaxValueExitReport = liveReport.ExitDbReport.Count > 0 ? _statisticsHelper.FindNextBestValueinTens(liveReport.ExitDbReport.Max(a => a.Count)) : 0,
                    ExitReport = liveReport.ExitDbReport.Count > 0 ? _statisticsHelper.GetGraphResponse_v1(liveReport.ExitDbReport,
                    "daily", CurrentDate) : null,
                    HourlyRevenue = liveReport.HourlyRevenue,
                    MonthlyRevenue = liveReport.MonthlyRevenue,
                    NoofTransactions = liveReport.NoofTransactions,
                    NoofVehiclesEntered = liveReport.NoofVehiclesEntered
                };

                return response;

            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public OccupancyGridGraphModel OccupancyGraph(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_OccupancyGraph_v2");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);
                objCmd.Parameters.AddWithValue("@pageNo", pageNo);
                objCmd.Parameters.AddWithValue("@pageSize", pageSize);
                objCmd.Parameters.AddWithValue("@sortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@sortOrder", sortOrder);

                DataSet ds = objSQL.FetchDB(objCmd);

                var occupancyReport = (from DataRow dr in ds.Tables[0].Rows
                                       select new OccupancyReport
                                       {
                                           BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                           Variation = Filter == "14dayforecast" ? _statisticsHelper.GetDateFormat(Convert.ToDateTime(dr["SlotDate"])) : Convert.ToString(dr["SlotTime"]),
                                           OccupancyCount = Convert.ToInt32(dr["OccupancyCount"])
                                       }).ToList();

                var bookingList = (from DataRow dr in ds.Tables[1].Rows
                                   select new BookingListModel
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       StartDate = Convert.ToDateTime(dr["StartDate"]),
                                       EndDate = Convert.ToDateTime(dr["EndDate"]),
                                       Duration = _statisticsHelper.GetDateDifference(Convert.ToDateTime(dr["StartDate"]), Convert.ToDateTime(dr["EndDate"]), Convert.ToInt32(dr["BookingTypeId"]) == 1 ? false : true),
                                       NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                                       NumberPlate = Convert.ToString(dr["NumberPlate"])
                                   }).ToList();


                var Finaloccupancyreport = new OccupancyGridGraphModel
                {
                    BookingList = bookingList,
                    MaxScaleValue = occupancyReport.Count > 0 ? _statisticsHelper.FindNextBestValueinTens(occupancyReport.Max(a => a.OccupancyCount)) : 0,
                    Total = bookingList.Count > 0 ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalCount"]) : 0
                };



                Finaloccupancyreport.OccupancyReport = _statisticsHelper.GetGraphResponse_v1(occupancyReport, Filter == "14dayforecast" ? Filter : "daily", CurrentDate);

                return Finaloccupancyreport;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public OccupancyGridGraphModel OccupancyGraphv1(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
        {

            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_OccupancyGraph_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);
                objCmd.Parameters.AddWithValue("@pageNo", pageNo);
                objCmd.Parameters.AddWithValue("@pageSize", pageSize);
                objCmd.Parameters.AddWithValue("@sortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@sortOrder", sortOrder);

                DataSet ds = objSQL.FetchDB(objCmd);

                var occupancyReport = (from DataRow dr in ds.Tables[0].Rows
                                       select new OccupancyReport
                                       {
                                           BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                           Variation = Filter == "14dayforecast" ? _statisticsHelper.GetDateFormat(Convert.ToDateTime(dr["SlotDate"])) : Convert.ToString(dr["SlotTime"]),
                                           OccupancyCount = Convert.ToInt32(dr["OccupancyCount"])
                                       }).ToList();

                var bookingList = (from DataRow dr in ds.Tables[1].Rows
                                   select new BookingListModel
                                   {
                                       BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                       StartDate = Convert.ToDateTime(dr["StartDate"]),
                                       EndDate = Convert.ToDateTime(dr["EndDate"]),
                                       Duration = _statisticsHelper.GetDateDifference(Convert.ToDateTime(dr["StartDate"]), Convert.ToDateTime(dr["EndDate"]), Convert.ToInt32(dr["BookingTypeId"]) == 1 ? false : true),
                                       NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                                       NumberPlate = Convert.ToString(dr["NumberPlate"])
                                   }).ToList();


                var Finaloccupancyreport = new OccupancyGridGraphModel
                {
                    BookingList = bookingList,
                    MaxScaleValue = occupancyReport.Count > 0 ? _statisticsHelper.FindNextBestValueinTens(occupancyReport.Max(a => a.OccupancyCount)) : 0,
                    Total = Convert.ToInt32(ds.Tables[1].Rows[0]["TotalCount"])
                };


                if (Filter == "14dayforecast")
                {
                    Finaloccupancyreport.OccupancyReport = _statisticsHelper.GetGraphResponse(occupancyReport, Filter, CurrentDate);
                }
                else
                {
                    Finaloccupancyreport.OccupancyReport = _statisticsHelper.GetOccupancyDailyResponse(occupancyReport);
                }
                return Finaloccupancyreport;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public GraphResponseModel RevenueGraph(long ParkingLocationId, string CurrentDate, string Filter)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_RevenueGraph");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);

                DataTable dtRevenue = objSQL.FetchDT(objCmd);

                var revenueReport = (from DataRow dr in dtRevenue.Rows
                                     select new RevenueReport
                                     {
                                         BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                         Amount = Convert.ToDecimal(dr["Amount"]),
                                         Interval = _statisticsHelper.GraphInterval(Filter, CurrentDate, dr)
                                     }).ToList();
                return _statisticsHelper.GetGraphResponse(revenueReport, Filter, CurrentDate);



            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public GraphResponseModel RevenueGraph_v1(long ParkingLocationId, string CurrentDate, string Filter)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_RevenueGraph_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);

                DataTable dtRevenue = objSQL.FetchDT(objCmd);

                var revenueReport = (from DataRow dr in dtRevenue.Rows
                                     select new RevenueReport
                                     {
                                         BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                         Amount = Convert.ToDecimal(dr["Amount"]),
                                         Interval = _statisticsHelper.GraphInterval_v1(Filter, CurrentDate, dr)
                                     }).ToList();
                return _statisticsHelper.GetGraphResponse_v1(revenueReport, Filter, CurrentDate);



            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public GraphResponseModel TransactionsGraph(long ParkingLocationId, string CurrentDate, string Filter)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_TransactionsGraph");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);

                DataTable dtRevenue = objSQL.FetchDT(objCmd);

                var transactionsReport = (from DataRow dr in dtRevenue.Rows
                                          select new TransactionsReport
                                          {
                                              BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                              Transactions = Convert.ToInt64(dr["Transactions"]),
                                              Interval = _statisticsHelper.GraphInterval(Filter, CurrentDate, dr)
                                          }).ToList();
                return _statisticsHelper.GetGraphResponse(transactionsReport, Filter, CurrentDate);

                // return new RevenueGraphModel { Report = revenueReport };

            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public GraphResponseModel TransactionsGraph_v1(long ParkingLocationId, string CurrentDate, string Filter)
        {
            Filter = Filter.ToLower();
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_TransactionsGraph_v1");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@Filter", Filter);

                DataTable dtRevenue = objSQL.FetchDT(objCmd);

                var transactionsReport = (from DataRow dr in dtRevenue.Rows
                                          select new TransactionsReport
                                          {
                                              BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                              Transactions = Convert.ToInt64(dr["Transactions"]),
                                              Interval = _statisticsHelper.GraphInterval_v1(Filter, CurrentDate, dr)
                                          }).ToList();
                return _statisticsHelper.GetGraphResponse_v1(transactionsReport, Filter, CurrentDate);

                // return new RevenueGraphModel { Report = revenueReport };

            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
    }
}