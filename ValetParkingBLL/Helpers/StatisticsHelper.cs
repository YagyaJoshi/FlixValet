using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AutoMapper;
using ValetParkingDAL.Models.StatisticsModels;

namespace ValetParkingBLL.Helpers
{
    public class StatisticsHelper
    {
        private readonly DateTimeHelper _dateTimeHelper;
        private readonly ParkingHelper _parkingHelper;
        private readonly IMapper _mapper;

        public StatisticsHelper(DateTimeHelper dateTimeHelper, ParkingHelper parkingHelper, IMapper mapper)
        {
            _dateTimeHelper = dateTimeHelper;
            _parkingHelper = parkingHelper;
            _mapper = mapper;
            var config = new MapperConfiguration(cfg =>
             {
                 cfg.CreateMap<LiveReportDbModel, LiveReportModel>();
             });
            _mapper = config.CreateMapper();
        }
        public OccupancyDailyResponse GetOccupancyDailyResponse(List<OccupancyReport> report)
        {

            OccupancyDailyResponse occupancyDaily = new OccupancyDailyResponse();
            occupancyDaily.HourlyReport = report.Where(a => a.BookingType == "hourly").Select(a => new OccupancyDailyReport
            {
                Variation = a.Variation,
                OccupancyCount = a.OccupancyCount
            }).ToList();

            occupancyDaily.MonthlyReport = report.Where(a => a.BookingType == "monthly").Select(a => new OccupancyDailyReport
            {
                Variation = a.Variation,
                OccupancyCount = a.OccupancyCount
            }).ToList();

            return occupancyDaily;
        }

        public LiveReportModel GetLiveReportResponse(LiveReportDbModel report)
        {
            LiveReportModel liveReport = new LiveReportModel();
            liveReport = _mapper.Map<LiveReportModel>(report);

            liveReport.EnterReport = new EnterExitResponse();
            liveReport.EnterReport.HourlyReport = report.EntryDbReport.Where(a => a.BookingType == "hourly").Select(a => new EnterExitDailyReport
            {
                Variation = a.Variation,
                Count = a.Count
            }).ToList();

            liveReport.EnterReport.MonthlyReport = report.EntryDbReport.Where(a => a.BookingType == "monthly").Select(a => new EnterExitDailyReport
            {
                Variation = a.Variation,
                Count = a.Count
            }).ToList();


            liveReport.EnterReport.MaxScaleValue = report.EntryDbReport.Count > 0 ? FindNextBestValueinTens(report.EntryDbReport.Max(a => a.Count)) : 0;

            liveReport.ExitReport = new EnterExitResponse();
            liveReport.ExitReport.HourlyReport = report.ExitDbReport.Where(a => a.BookingType == "hourly").Select(a => new EnterExitDailyReport
            {
                Variation = a.Variation,
                Count = a.Count
            }).ToList();

            liveReport.ExitReport.MonthlyReport = report.ExitDbReport.Where(a => a.BookingType == "monthly").Select(a => new EnterExitDailyReport
            {
                Variation = a.Variation,
                Count = a.Count
            }).ToList();

            liveReport.ExitReport.MaxScaleValue = report.ExitDbReport.Count > 0 ? FindNextBestValueinTens(report.ExitDbReport.Max(a => a.Count)) : 0;

            return liveReport;
        }

        public static DateTime FirstDayOfWeek(DateTime date)
        {
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }

        public static DateTime LastDayOfWeek(DateTime date)
        {
            DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
            return ldowDate;
        }

        public LiveReportV1Model LiveReportResponse(LiveReportDbModel report)
        {
            LiveReportV1Model liveReport = new LiveReportV1Model();

            string[] timeArray = new string[24];
            int[] enterHourlyReport, enterMonthlyReport, exitHourlyReport, exitMonthlyReport;


            enterHourlyReport = enterMonthlyReport = exitHourlyReport = exitMonthlyReport = new int[24];
            TimeSpan startTime = new TimeSpan(1, 00, 00);
            for (int i = 0; i < 24; i++)
            {
                timeArray[i] = (startTime.TotalHours == 24 ? "00:00:00" : startTime.ToString());
                startTime += TimeSpan.FromHours(1);
            }

            for (int i = 0; i < 24; i++)
            {
                TimeSpan fromTime, ToTime;
                fromTime = ToTime = TimeSpan.Parse(timeArray[i]);
                fromTime = TimeSpan.FromHours(-1);

                enterHourlyReport[i] = (report.EntryDbReport.Where(a => a.BookingType == "hourly" && (ToTime != TimeSpan.Parse("00:00:00") && TimeSpan.Parse(a.Variation) <= ToTime) && TimeSpan.Parse(a.Variation) > fromTime)).Sum(a => a.Count);

                enterMonthlyReport[i] = (report.EntryDbReport.Where(a => a.BookingType == "monthly" && (ToTime != TimeSpan.Parse("00:00:00") && TimeSpan.Parse(a.Variation) <= ToTime) && TimeSpan.Parse(a.Variation) > fromTime)).Sum(a => a.Count);

                exitHourlyReport[i] = (report.ExitDbReport.Where(a => a.BookingType == "hourly" && (ToTime != TimeSpan.Parse("00:00:00") && TimeSpan.Parse(a.Variation) <= ToTime) && TimeSpan.Parse(a.Variation) > fromTime)).Sum(a => a.Count);

                exitMonthlyReport[i] = (report.ExitDbReport.Where(a => a.BookingType == "monthly" && (ToTime != TimeSpan.Parse("00:00:00") && TimeSpan.Parse(a.Variation) <= ToTime) && TimeSpan.Parse(a.Variation) > fromTime)).Sum(a => a.Count);


            }

            return liveReport;

        }
        //  public int LiveBookingTypewiseCount()
        public GraphResponseModel GetGraphResponse<T>(List<T> Report, string Filter, string CurrentDate, int MaxDuration = 0)
        {
            List<string> WeekLog = new List<string>();

            GraphResponseModel obj = new GraphResponseModel();


            if (Filter == "weekly")
            {
                obj.Hourly = new decimal[4];
                obj.Monthly = new decimal[4];
                for (int i = 0; i <= 3; i++)
                {
                    DateTime first = FirstDayOfWeek(DateTime.Now.AddDays(-7 * i));
                    DateTime last = LastDayOfWeek(DateTime.Now.AddDays(-7 * i));

                    WeekLog.Add(_dateTimeHelper.GetDateFormat(first) + "-" + _dateTimeHelper.GetDateFormat(last));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }

            else if (Filter == "monthly")
            {
                obj.Hourly = new decimal[12];
                obj.Monthly = new decimal[12];

                for (int i = 0; i <= 11; i++)
                {
                    DateTime date = new DateTime(Convert.ToDateTime(CurrentDate).Year, i + 1, 1);
                    WeekLog.Add(date.ToString("MMM"));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    #region reference
                    // ((i == 0 && a.Interval.Equals(WeekLog[0])) ||
                    //  (i == 1 && a.Interval.Equals(WeekLog[1])) ||
                    //  (i == 2 && a.Interval.Equals(WeekLog[2])) ||
                    //  (i == 3 && a.Interval.Equals(WeekLog[3])) ||
                    //  (i == 4 && a.Interval.Equals(WeekLog[4])) ||
                    //  (i == 5 && a.Interval.Equals(WeekLog[5])) ||
                    //  (i == 6 && a.Interval.Equals(WeekLog[6])) ||
                    //  (i == 7 && a.Interval.Equals(WeekLog[7])) ||
                    //  (i == 8 && a.Interval.Equals(WeekLog[8])) ||
                    //  (i == 9 && a.Interval.Equals(WeekLog[9])) ||
                    //  (i == 10 && a.Interval.Equals(WeekLog[10])) ||
                    //  (i == 11 && a.Interval.Equals(WeekLog[11])))).Select(x =>
                    //    x.Amount
                    //  ).FirstOrDefault();
                    #endregion

                    obj.Categories = WeekLog.ToArray();

                }

            }

            else if (Filter == "currentweek")
            {
                obj.Hourly = new decimal[7];
                obj.Monthly = new decimal[7];

                for (int i = 0; i <= 6; i++)
                {

                    WeekLog.Add(_dateTimeHelper.GetDateFormat(Convert.ToDateTime(CurrentDate).AddDays(i - 6)));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }
            else if (Filter == "14dayforecast")
            {
                obj.Hourly = new decimal[14];
                obj.Monthly = new decimal[14];

                for (int i = 0; i <= 13; i++)
                {

                    WeekLog.Add(_dateTimeHelper.GetDateFormat(Convert.ToDateTime(CurrentDate).AddDays(i)));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }
            else
            {
                int arrSize = MaxDuration / 2;
                arrSize = arrSize < 12 ? 12 : arrSize;
                obj.Hourly = new decimal[arrSize];
                obj.Monthly = new decimal[arrSize];

                for (int i = 1; i <= arrSize; i++)
                {
                    WeekLog.Add((2 * i).ToString());

                    obj.Hourly[i - 1] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, 2 * i);


                    obj.Monthly[i - 1] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, 2 * i);

                    obj.Categories = WeekLog.ToArray();

                }

            }
            return obj;
        }


        public GraphResponseModel GetGraphResponse_v1<T>(List<T> Report, string Filter, string CurrentDate)
        {
            List<string> WeekLog = new List<string>();

            GraphResponseModel obj = new GraphResponseModel();


            if (Filter == "weekly")
            {
                obj.Hourly = new decimal[7];
                obj.Monthly = new decimal[7];
                DateTime FirstDay = FirstDayOfWeek(Convert.ToDateTime(CurrentDate));

                for (int i = 0; i <= 6; i++)
                {

                    // WeekLog.Add(_dateTimeHelper.GetDateFormat(Convert.ToDateTime(CurrentDate).AddDays(i - 6)));


                    WeekLog.Add(_dateTimeHelper.GetDateFormat(FirstDay.AddDays(i)));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }

            else if (Filter == "monthly")
            {
                obj.Hourly = new decimal[12];
                obj.Monthly = new decimal[12];

                for (int i = 0; i <= 11; i++)
                {
                    DateTime date = new DateTime(Convert.ToDateTime(CurrentDate).Year, i + 1, 1);
                    WeekLog.Add(date.ToString("MMM"));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);


                    obj.Categories = WeekLog.ToArray();

                }

            }

            else if (Filter == "currentweek")
            {
                obj.Hourly = new decimal[7];
                obj.Monthly = new decimal[7];

                for (int i = 0; i <= 6; i++)
                {

                    WeekLog.Add(_dateTimeHelper.GetDateFormat(Convert.ToDateTime(CurrentDate).AddDays(i - 6)));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }
            else if (Filter == "14dayforecast")
            {
                obj.Hourly = new decimal[14];
                obj.Monthly = new decimal[14];

                for (int i = 0; i <= 13; i++)
                {

                    WeekLog.Add(_dateTimeHelper.GetDateFormat(Convert.ToDateTime(CurrentDate).AddDays(i)));

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);


                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                    obj.Categories = WeekLog.ToArray();
                }
            }
            else if (Filter == "DurationGraph")
            {
                obj.Hourly = new decimal[12];
                obj.Monthly = new decimal[12];

                for (int i = 1; i <= 12; i++)
                {
                    WeekLog.Add((2 * i).ToString());

                    obj.Hourly[i - 1] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, 2 * i);


                    obj.Monthly[i - 1] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, 2 * i);

                    obj.Categories = WeekLog.ToArray();
                }

            }
            else
            {
                obj.Hourly = new decimal[24];
                obj.Monthly = new decimal[24];

                TimeSpan startTime = new TimeSpan(0, 0, 0);
                for (int i = 0; i < 24; i++)
                {

                    WeekLog.Add(startTime.ToString().Remove(startTime.ToString().LastIndexOf(':')));
                    startTime += TimeSpan.FromHours(1);

                    obj.Hourly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "hourly", WeekLog, i);

                    obj.Monthly[i] = GetCategorizedRecord(Report, Filter, CurrentDate, "monthly", WeekLog, i);

                }

                obj.Categories = WeekLog.ToArray();
            }
            return obj;
        }

        public dynamic GetCategorizedRecord<T>(List<T> Report, string Filter, string CurrentDate, string BookingType, List<string> WeekLog, int index)
        {

            Type listType = typeof(T);
            if (listType == typeof(RevenueReport))
            {
                var revenueReport = Report as List<RevenueReport>;
                return revenueReport.Where(a => a.BookingType == BookingType && a.Interval.Equals(WeekLog[index])).Select(x =>
                            x.Amount
                          ).FirstOrDefault();
            }
            else if (listType == typeof(TransactionsReport))
            {
                var transactionsReport = Report as List<TransactionsReport>;
                return transactionsReport.Where(a => a.BookingType == BookingType && a.Interval.Equals(WeekLog[index])).Select(x =>
                            x.Transactions
                          ).FirstOrDefault();
            }

            else if (listType == typeof(OccupancyReport))
            {
                var occupancyReport = Report as List<OccupancyReport>;
                if (Filter == "daily")
                {
                    return occupancyReport.Where(a => a.BookingType == BookingType && (new TimeSpan(Convert.ToInt32(a.Variation), 0, 0)).Equals(TimeSpan.Parse(WeekLog[index]))).Select(x =>
                                                 x.OccupancyCount
                                             ).FirstOrDefault();
                }
                else
                {

                    return occupancyReport.Where(a => a.BookingType == BookingType && a.Variation.Equals(WeekLog[index])).Select(x =>
                                x.OccupancyCount
                              ).FirstOrDefault();
                }
            }

            else if (listType == typeof(DurationReport))
            {
                var durationReport = Report as List<DurationReport>;

                if (index == 2)
                    return (durationReport.Where(a => a.BookingType == BookingType && a.Interval >= index - 2 && a.Interval <= index)).Sum(a => a.BookingsCount);

                else
                    return (durationReport.Where(a => a.BookingType == BookingType && a.Interval > index - 2 && a.Interval <= index)).Sum(a => a.BookingsCount);

                // return durationReport.Where(a => a.BookingType == BookingType && a.Interval.Equals(WeekLog[index])).Select(x =>
                //    x.OccupancyCount
                // ).FirstOrDefault();
            }

            else if (listType == typeof(LiveReport))
            {
                var liveReport = Report as List<LiveReport>;
                return liveReport.Where(a => a.BookingType == BookingType && (new TimeSpan(Convert.ToInt32(a.Variation), 0, 0)).Equals(TimeSpan.Parse(WeekLog[index]))).Select(x =>
                              x.Count
                          ).FirstOrDefault();
            }
            return null;
        }
        public decimal calculatePercentage(dynamic ThisYearReport, dynamic LastYearReport)
        {

            return LastYearReport == 0 ? 100 : _parkingHelper.RoundOff(((ThisYearReport - LastYearReport) * 100) / LastYearReport);
        }

        public decimal calculatePieGraphPercentage(long OccupiedSlots, long No_of_Spaces, int days)
        {
            decimal percentge = _parkingHelper.RoundOff((decimal)(OccupiedSlots * 100) / (No_of_Spaces * days));
            //return 
            return percentge;

        }
        public decimal calculatePieGraphPercentageV1(long OccupiedSlots, long Total)
        {
            decimal percentage = _parkingHelper.RoundOff((decimal)(OccupiedSlots * 100) / Total);
            return percentage;
        }

        public string GraphInterval(string Filter, string CurrentDate, DataRow dr)
        {

            switch (Filter)
            {
                case "monthly":
                    return new DateTime(Convert.ToDateTime(CurrentDate).Year, Convert.ToInt32(dr["Month"]), 1).ToString("MMM");
                case "weekly":
                    return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["FromDate"])) + "-" + _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["ToDate"]));
                case "currentweek":
                    return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["Date"]));
                default:
                    return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["Date"]));
            };
        }

        public string GraphInterval_v1(string Filter, string CurrentDate, DataRow dr)
        {

            switch (Filter)
            {
                case "monthly":
                    return new DateTime(Convert.ToDateTime(CurrentDate).Year, Convert.ToInt32(dr["Month"]), 1).ToString("MMM");
                // case "weekly" :
                //     return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["FromDate"])) + "-" + _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["ToDate"]));
                case "currentweek":
                case "weekly":
                    return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["Date"]));
                default:
                    return _dateTimeHelper.GetDateFormat(Convert.ToDateTime(dr["Date"]));
            };
        }

        public string GetDateDifference(DateTime StartDate, DateTime EndDate, bool IsMonthly = false)
        {
            return _dateTimeHelper.GetDateDifference(StartDate, EndDate, IsMonthly);
        }

        public decimal RoundOff(dynamic value)
        {
            return _parkingHelper.RoundOff(value);
        }

        public string GetDateFormat(DateTime Date)
        {
            return _dateTimeHelper.GetDateFormat(Date);
        }

        public int FindNextBestValueinTens(int num)
        {
            int LastDigit = num % 10;
            int RoundedValue = num;
            if (LastDigit != 0)
            {
                num = num / 10;
                RoundedValue = (num + 1) * 10;
            }

            return RoundedValue;

        }

        public int FindNextBestEvenValue(int num)
        {

            bool IsEven = num % 2 == 0;

            return IsEven ? num : num + 1;

        }

    }
}