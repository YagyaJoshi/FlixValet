using System;
using System.Text;

namespace ValetParkingBLL.Helpers
{
    public class DateTimeHelper
    {
        public string GetDateFormatBasedonCurrentDate(DateTime Date, DateTime CurrentDate, bool NeedShowToday = true)
        {

            if (Date.Date == CurrentDate)
            {
                return NeedShowToday ? $"Today at {Date:hh:mm tt}" : $"{Date:hh:mm tt}";
            }

            else if (Date.Date == CurrentDate.AddDays(-1))
            {
                return $"Yesterday at {Date:hh:mm tt}";
            }
            else
            {
                return $"{Date:MMM dd hh:mm tt}";
            }

        }
        public string GetDateWithTimeFormat(DateTime Date)
        {
            return $"{Date:MMM dd hh:mm tt}";
        }

        public string GetDateFormat(DateTime Date)
        {
            return $"{Date:MMM dd}";
        }

        public string GetDateDifference(DateTime StartDate, DateTime EndDate, bool IsMonthly = false)
        {
            TimeSpan span = (EndDate - StartDate);

            StringBuilder sb = new StringBuilder();

            if (IsMonthly)
            {
                span = (StartDate.Date + EndDate.TimeOfDay) - StartDate;
                sb.Append(span.Hours + (span.Hours > 1 ? " Hrs " : " Hr "));
            }
            else
            {
                if (span.Days > 0)
                    sb.Append(span.Days + (span.Days > 1 ? " Days " : " Day "));
                if (span.Hours > 0)
                    sb.Append(span.Hours + (span.Hours > 1 ? " Hrs " : " Hr "));
                if (span.Minutes > 0)
                    sb.Append(span.Minutes + (span.Minutes > 1 ? " Mins " : " Min "));
            }
            return sb.ToString();
        }

    }
}