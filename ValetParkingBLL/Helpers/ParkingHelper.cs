using System.Collections.Generic;
using ValetParkingDAL.Models;
using System.Linq;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Enums;
using System;
using ValetParkingDAL.Models.CustomerModels;
using System.Text.RegularExpressions;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Helpers
{
    public class ParkingHelper
    {

        public List<List<ParkingLocationTimingRequest>> GetTimings(List<ParkingLocationTiming> lstTimings)
        {
            //  var dayTimings = lstTimings.GroupBy(a => new { a.IsMonday, a.IsTuesday, a.IsWednesday, a.IsThursday, a.IsFriday, a.IsSaturday, a.IsSunday }).Select(a =>a);


            List<List<ParkingLocationTimingRequest>> list = new List<List<ParkingLocationTimingRequest>>();

            for (int i = 0; i <= 6; i++)
            {
                list.Add(lstTimings.Where(a => (i == 0 && a.IsMonday.Equals(true)) ||
                (i == 1 && a.IsTuesday.Equals(true)) ||
                (i == 2 && a.IsWednesday.Equals(true)) ||
                (i == 3 && a.IsThursday.Equals(true)) ||
                (i == 4 && a.IsFriday.Equals(true)) ||
                (i == 5 && a.IsSaturday.Equals(true)) ||
                (i == 6 && a.IsSunday.Equals(true))).Select(x => new ParkingLocationTimingRequest
                {
                    StartTime = x.StartTime.ToString(),
                    EndTime = x.EndTime.ToString(),
                }).ToList());
            }


            // foreach (var item in dayTimings)
            // {

            //     list.Add(item.Select(x => new ParkingLocationTimingRequest
            //     {
            //         StartTime = x.StartTime.ToString(),
            //         EndTime = x.EndTime.ToString(),
            //     }).ToList());
            // }
            return list;



        }


        public List<List<ParkingLocationRateRequest>> GetRates(List<ParkingLocationRate> lstRates)
        {
            var rates = lstRates.GroupBy(a => a.BookingType).Select(a => a);

            List<List<ParkingLocationRateRequest>> list = new List<List<ParkingLocationRateRequest>>();
            foreach (var item in rates)
            {
                list.Add(item.Select(x => new ParkingLocationRateRequest
                {
                    Duration = x.Duration,
                    Charges = x.Charges
                }).ToList());
            }
            return list;
        }
        public List<ParkingLocationTiming> GetParkingTimingList(List<List<ParkingLocationTimingRequest>> model, DateTime StartDate, string TimeZone)
        {

            DateTime CurrentDate = DateTime.Parse(StartDate.ToShortDateString());

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
            // bool IsDaylightSavingTime = timeZoneInfo.IsDaylightSavingTime(StartDate);
            List<ParkingLocationTiming> lstTimings = new List<ParkingLocationTiming>();
            ParkingLocationTiming timingRequest = new ParkingLocationTiming();

            for (int i = 0; i < model.Count; i++)
            {
                foreach (var time in model[i].ToList())
                {
                    timingRequest = new ParkingLocationTiming();

                    timingRequest.IsMonday = i == (int)EDays.Monday ? true : false;
                    timingRequest.IsTuesday = i == (int)EDays.Tuesday ? true : false;
                    timingRequest.IsWednesday = i == (int)EDays.Wednesday ? true : false;
                    timingRequest.IsThursday = i == (int)EDays.Thursday ? true : false;
                    timingRequest.IsFriday = i == (int)EDays.Friday ? true : false;
                    timingRequest.IsSaturday = i == (int)EDays.Saturday ? true : false;
                    timingRequest.IsSunday = i == (int)EDays.Sunday ? true : false;
                    timingRequest.StartDate = CurrentDate;
                    timingRequest.StartDateUtc = TimeZoneInfo.ConvertTimeToUtc(CurrentDate, timeZoneInfo);

                    timingRequest.StartTime = TimeSpan.Parse(time.StartTime);

                    DateTime dateStart = CurrentDate + timingRequest.StartTime;
                    timingRequest.StartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateStart, timeZoneInfo).TimeOfDay;

                    //use this if ConvertTimeToUtc gives error.
                    // IsDaylightSavingTime?dateStart.ToUniversalTime().AddHours(1).TimeOfDay : dateStart.ToUniversalTime().TimeOfDay;

                    dateStart = CurrentDate + TimeSpan.Parse(time.EndTime);
                    timingRequest.EndTime = TimeSpan.Parse(time.EndTime);
                    timingRequest.EndTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateStart, timeZoneInfo).TimeOfDay;

                    lstTimings.Add(timingRequest);
                }

            }

            return lstTimings;
        }


        public List<DateTime> GetDateList(DateTime StartDate, DateTime EndDate)
        {
            List<DateTime> SearchDates = new List<DateTime>();
            for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
                SearchDates.Add(date);
            return SearchDates;
        }
        public (List<BookingDetails>, AdditionalChargesModel) AdditionalPaymentLogic(BookingDetailResponse model, DateTime CurrentDate, bool IsEarlyBirdOfferApplied, List<ParkingLocationRateRequest> rates)
        {
            List<BookingDetails> bookingList = new List<BookingDetails>();
            bool IsMonthlyBooking = model.BookingType.ToString().ToLower().Equals("monthly");

            decimal BookingSlab = 0.00m, PerHourRate = 0.00m, NewSlabCharges = 0.00m, AdditionalStayDuration = 0.00m, UnSettledCharges = 0.00m, FinalBookingDuration = 0.00m, NewBookingAmount = 0.00m, TaxAmt = 0.00m;

            double NewSlabDuration = 0; int DeductedDurationCount = 0;
            DateTime StartDate, EnterDate, EndDate, ExitDate;
            TimeSpan FromTime, ToTime;


            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
            string ExitTime;

            ExitTime = string.IsNullOrEmpty(model.ExitDate) ? CurrentDate.TimeOfDay.ToString() : model.ExitDate;

            StartDate = model.StartDate + TimeSpan.Parse(model.StartTime);
            EnterDate = DateTime.Parse(model.EntryDate) + TimeSpan.Parse(model.EnterTime);
            EndDate = model.EndDate + TimeSpan.Parse(model.EndTime);
            ExitDate = string.IsNullOrEmpty(model.ExitDate) ? CurrentDate : DateTime.Parse(model.ExitDate) + TimeSpan.Parse(model.ExitTime);

            bool IsStayedContinuousInMonthly = IsMonthlyBooking && EnterDate.Date != ExitDate.Date;

            List<DateTime> Dates = GetDateList(EnterDate.Date, ExitDate.Date);
            BookingDetails objBooking = new BookingDetails();

            BookingSlab = Convert.ToDecimal(model.MaxDurationofSlab);



            #region old code use for reference

            // if (Dates.Count == 1)
            // {
            //     Duration = EnterDateSlab;
            //     if ((StartDate.TimeOfDay > EnterDate.TimeOfDay || EndDate.TimeOfDay < ExitDate.TimeOfDay))
            //     {
            //         FromTime = EnterDate.TimeOfDay;
            //         //EnterDate.TimeOfDay < StartDate.TimeOfDay ? EnterDate.TimeOfDay : StartDate.TimeOfDay;
            //         ToTime = ExitDate.TimeOfDay;

            //         if (Convert.ToDecimal((ExitDate - EnterDate).TotalHours) > Duration)
            //             objBooking = CreateBookingDetailObject(StartDate.Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, !StartDate.Date.Equals(EndDate.Date) ? Convert.ToDecimal((ExitDate - EnterDate).TotalHours) - BookingSlab : BookingSlab, model.Charges, false, StartDate.Date.Equals(EndDate.Date) ? false : true);
            //         else
            //             objBooking = CreateBookingDetailObject(StartDate.Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, BookingSlab, model.Charges, true);
            //         bookingList.Add(objBooking);
            //     }

            // }
            // else
            // {
            //     bool isTakenForHourly = false;
            //     foreach (var Date in Dates)
            //     {
            //         if (model.BookingType.ToLower().Equals("monthly"))
            //         {
            //             if (Date.Equals(EnterDate.Date))
            //             {
            //                 FromTime = EnterDate.TimeOfDay < StartDate.TimeOfDay ? EnterDate.TimeOfDay : StartDate.TimeOfDay;
            //                 ToTime = TimeSpan.Parse("23:59:59");
            //                 objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, BookingSlab);
            //             }
            //             else
            //             {
            //                 FromTime = TimeSpan.Parse("00:00:00");
            //                 ToTime = Date.Equals(ExitDate.Date) ? ExitDate.TimeOfDay : TimeSpan.Parse("23:59:59");
            //                 objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, Date <= EndDate.Date ? BookingSlab : 0.00m);
            //             }
            //             bookingList.Add(objBooking);
            //         }
            //         else
            //         {
            //             bool IsDayChanged = false;
            //             IsDayChanged = !StartDate.Date.Equals(EndDate.Date) || !EnterDate.Date.Equals(ExitDate);
            //             if (Date.Equals(EnterDate.Date) && Date.Equals(StartDate.Date))
            //             {

            //                 FromTime = EnterDate.TimeOfDay < StartDate.TimeOfDay ? EnterDate.TimeOfDay : StartDate.TimeOfDay;
            //                 ToTime = TimeSpan.Parse("23:59:59");
            //                 if (IsDayChanged && EnterDate >= StartDate && !StartDate.Date.Equals(EndDate.Date)) { }
            //                 else
            //                 {
            //                     isTakenForHourly = IsDayChanged ? true : false;
            //                     objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, IsDayChanged ? Convert.ToDecimal((ExitDate - EnterDate).TotalHours) - BookingSlab : EnterDateSlab, 0.00m, false, IsDayChanged ? true : false);
            //                     bookingList.Add(objBooking);
            //                 }

            //             }

            //             if (!Date.Equals(StartDate.Date) && Date >= EndDate.Date)
            //             {

            //                 FromTime = TimeSpan.Parse("00:00:00");
            //                 ToTime = Date.Equals(ExitDate.Date) ? ExitDate.TimeOfDay : TimeSpan.Parse("23:59:59");
            //                 if (Date.Equals(ExitDate.Date) && Date.Equals(EndDate.Date) && ExitDate <= EndDate) { }
            //                 else
            //                 {
            //                     if (IsDayChanged)
            //                     {
            //                         Duration = isTakenForHourly ? 0.00m : Convert.ToDecimal((ExitDate - EnterDate).TotalHours) - BookingSlab;
            //                         isTakenForHourly = true;
            //                     }


            //                     objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, IsDayChanged ? Duration : (Date.Equals(EndDate.Date) ? Convert.ToDecimal((EndDate.TimeOfDay - TimeSpan.Parse("00:00:00")).TotalHours) : 0.00m), 0.00m, IsDayChanged && Duration.Equals(0.00m) ? true : false, isTakenForHourly);
            //                     bookingList.Add(objBooking);

            //                 }

            //             }
            //         }
            //     }
            // }

            #endregion

            #region Old code, will be used if client asks for it.
            if (IsStayedContinuousInMonthly)
            {

                foreach (var Date in Dates)
                {
                    if (Date.Equals(EnterDate.Date))
                    {
                        FromTime = (StartDate.TimeOfDay <= EnterDate.TimeOfDay && EnterDate.TimeOfDay <= EndDate.TimeOfDay) ? StartDate.TimeOfDay : EnterDate.TimeOfDay;
                        ToTime = TimeSpan.Parse("23:59:59");
                        DeductedDurationCount = FromTime <= StartDate.TimeOfDay ? ++DeductedDurationCount : DeductedDurationCount;
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, FromTime <= StartDate.TimeOfDay ? model.Duration : 0.00m, 0.00m, true);
                    }
                    else if (Date > EnterDate.Date && Date < ExitDate.Date)
                    {
                        FromTime = TimeSpan.Parse("00:00:00");
                        ToTime = TimeSpan.Parse("23:59:59");
                        DeductedDurationCount = model.LastBookingDate >= Date ? ++DeductedDurationCount : DeductedDurationCount;
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, model.LastBookingDate >= Date ? model.Duration : 0.00m, 0.00m, true);
                    }
                    else
                    {
                        FromTime = TimeSpan.Parse("00:00:00");
                        ToTime = (StartDate.TimeOfDay <= ExitDate.TimeOfDay && ExitDate.TimeOfDay <= EndDate.TimeOfDay) ? EndDate.TimeOfDay : ExitDate.TimeOfDay;
                        DeductedDurationCount = ToTime >= StartDate.TimeOfDay && model.LastBookingDate >= Date ? ++DeductedDurationCount : DeductedDurationCount;
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, ToTime >= StartDate.TimeOfDay && model.LastBookingDate >= Date ? model.Duration : 0.00m, 0.00m, true);
                    }

                    bookingList.Add(objBooking);
                }

            }

            else
            {
                foreach (var Date in Dates)
                {
                    if (Date.Equals(EnterDate.Date) && StartDate > EnterDate)
                    {
                        FromTime = EnterDate.TimeOfDay;
                        ToTime = ExitDate > StartDate ? StartDate.TimeOfDay : ExitDate.TimeOfDay;
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, 0.00m, 0.00m, true);
                        bookingList.Add(objBooking);
                    }
                    if (!IsMonthlyBooking && Date >= EndDate.Date && ExitDate.Date > EndDate.Date)
                    {
                        FromTime = Date == EndDate.Date ? EndDate.TimeOfDay : TimeSpan.Parse("00:00:00");
                        ToTime = Date == ExitDate.Date ? ExitDate.TimeOfDay : TimeSpan.Parse("23:59:59");
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, 0.00m, 0.00m, true);
                        bookingList.Add(objBooking);

                    }
                    if (Date.Equals(ExitDate.Date) && ExitDate.Date == EndDate.Date && ExitDate.TimeOfDay > EndDate.TimeOfDay)
                    {
                        FromTime = EnterDate > EndDate ? EnterDate.TimeOfDay : EndDate.TimeOfDay;
                        ToTime = ExitDate.TimeOfDay;
                        objBooking = CreateBookingDetailObject(Date, timeZoneInfo, FromTime, ToTime, false, PerHourRate, 0.00m, 0.00m, true);
                        bookingList.Add(objBooking);
                    }
                }
            }

            #endregion


            AdditionalStayDuration = Convert.ToDecimal(bookingList.Sum(a => a.Duration));

            //Fetch actual booking hours.(not slab wise)
            var (TotalHours, perHourRate, rate) = GetTotalHoursandAmountByDuration(rates, StartDate.Date, EndDate.Date, model.StartTime, model.EndTime, IsMonthlyBooking ? false : true, true);

            //TotalStayDuration calculated including paid booking hours.
            FinalBookingDuration = IsStayedContinuousInMonthly ? AdditionalStayDuration + (TotalHours * DeductedDurationCount) : TotalHours + AdditionalStayDuration;

            NewSlabDuration = Convert.ToDouble(model.MaxDurationofSlab);
            NewSlabCharges = Convert.ToDecimal(model.MaxRateofSlab);

            //if total staying hours exceeded existing booking slab only then additional charges taken.
            if (FinalBookingDuration > BookingSlab && AdditionalStayDuration > 0)
            {
                //New Booking slab calculation,will be updated in database only in case of hourly booking.
                var (duration, charges) = FetchMaxDurationandCharges(rates, Convert.ToDouble(FinalBookingDuration));
                NewSlabDuration = duration;
                NewSlabCharges = charges;

                //unsettled charges will be new booking charges - paid booking charges. Suppose booking slab was of 3 hours and customer stayed for 5 hours then take additional amount by doing 5-3=2 hours.
                if (model.BookingCategoryId != ((int)EBookingCategories.NoCharge))
                {
                    UnSettledCharges = charges - (IsStayedContinuousInMonthly ?
                    Convert.ToDecimal(model.MaxRateofSlab) * DeductedDurationCount : Convert.ToDecimal(model.MaxRateofSlab));
                }
                else
                {
                    UnSettledCharges = 0;
                }
                UnSettledCharges = UnSettledCharges < 0 ? 0 : UnSettledCharges;
                bookingList.FirstOrDefault().Charges = UnSettledCharges;
            }

            NewBookingAmount = RoundOff(model.BookingAmount + UnSettledCharges);

            var ConvenienceFee = model.ConvenienceFee > 0 ? model.ConvenienceFee : ((model.PaymentMode.ToLower() == EPaymentMode.Electronic.ToString()) ? model.ConvenienceFee : 0);

            // TaxAmt = RoundOff(((NewBookingAmount + model.OverweightCharges + (string.IsNullOrEmpty(model.PaymentMode) ? 0 : ((model.PaymentMode.ToLower() == EPaymentMode.Cash.ToString().ToLower()) ? (((model.ConvenienceFee > 0) && (model.BookingCategoryId != 2)) ? model.ConvenienceFee : 0) : model.ConvenienceFee))) * model.TaxPercent) / 100);

            TaxAmt = RoundOff(((NewBookingAmount + model.OverweightCharges + (string.IsNullOrEmpty(model.PaymentMode) ? 0 : ConvenienceFee)) * model.TaxPercent) / 100);

            var TaxAmtWithConvenienceFee = model.ConvenienceFee > 0 ? TaxAmt : RoundOff(((NewBookingAmount + model.OverweightCharges + model.LocationConvenienceFee) * model.TaxPercent) / 100);


            AdditionalChargesModel OverStayResponse = new AdditionalChargesModel
            {
                rate = new ParkingLocationRateRequest { Duration = Convert.ToInt32(NewSlabDuration), Charges = NewSlabCharges },
                Duration = NewSlabDuration,
                Charges = NewSlabCharges,
                UnSettledCharges = UnSettledCharges,
                NewBookingAmount = NewBookingAmount,
                TaxAmount = TaxAmt,
                NetAmount = NewBookingAmount + model.OverweightCharges + TaxAmt + (string.IsNullOrEmpty(model.PaymentMode) ? 0 : ConvenienceFee),
                OverStayDuration = FinalBookingDuration - (IsStayedContinuousInMonthly ? BookingSlab * DeductedDurationCount : BookingSlab),
                TaxAmountWithConvenienceFee = TaxAmtWithConvenienceFee,
                NetAmountWithConvenienceFee = model.ConvenienceFee > 0 ? NewBookingAmount + model.OverweightCharges + TaxAmt + (string.IsNullOrEmpty(model.PaymentMode) ? 0 : ConvenienceFee) : NewBookingAmount + model.OverweightCharges + TaxAmtWithConvenienceFee + model.LocationConvenienceFee,
            };
            model.TaxAmount = OverStayResponse.TaxAmount;

            return (bookingList, OverStayResponse);
        }

        public (double, List<SearchParkingSlots>) GetSearchDateTimingwiseTable(CurrentLocationRequest model, TimeZoneInfo timeZoneInfo)
        {
            double TotalHours = 0.00;
            TimeSpan StartTimeSpan, EndTimeSpan;
            bool IsDayChanged = false;

            StartTimeSpan = TimeSpan.Parse(model.StartTime);
            EndTimeSpan = TimeSpan.Parse(model.EndTime);


            if (EndTimeSpan < StartTimeSpan) IsDayChanged = true;


            List<DateTime> SearchDates = GetDateList(model.StartDate, model.EndDate);


            List<SearchParkingSlots> SearchList = new List<SearchParkingSlots>();
            SearchParkingSlots objSlot;
            foreach (DateTime Date in SearchDates)
            {
                objSlot = new SearchParkingSlots();
                double Duration = 0.00;
                if (model.IsFullTimeBooking)
                {
                    objSlot = new SearchParkingSlots();
                    objSlot = GetSlotObject(objSlot, Date, timeZoneInfo, Date == model.StartDate ? StartTimeSpan : TimeSpan.Parse("00:00:00"), Date == model.EndDate ? EndTimeSpan : TimeSpan.Parse("23:59:59"), IsDayChanged, ref Duration);
                    TotalHours += Duration;
                    SearchList.Add(objSlot);
                }
                else
                {

                    if (IsDayChanged)
                    {
                        SearchParkingSlots objSlot1;
                        if (Date != model.EndDate)
                        {
                            Duration = 0.00;
                            objSlot1 = new SearchParkingSlots();
                            objSlot1 = GetSlotObject(objSlot1, Date, timeZoneInfo, StartTimeSpan, TimeSpan.Parse("23:59:59"), IsDayChanged, ref Duration);
                            TotalHours += Duration;
                            SearchList.Add(objSlot1);
                        }
                        if (Date != model.StartDate)
                        {
                            Duration = 0.00;
                            objSlot1 = new SearchParkingSlots();
                            objSlot1 = GetSlotObject(objSlot1, Date, timeZoneInfo, TimeSpan.Parse("00:00:00"), EndTimeSpan, IsDayChanged, ref Duration);
                            TotalHours += Duration;
                            SearchList.Add(objSlot1);
                        }
                    }
                    else
                    {
                        objSlot = GetSlotObject(objSlot, Date, timeZoneInfo, StartTimeSpan, EndTimeSpan, IsDayChanged, ref Duration);
                        TotalHours += Duration;
                        SearchList.Add(objSlot);
                    }
                }
            }
            return (TotalHours, SearchList);
        }

        public EarlyBirdInfo GetEarlyBirdInfo(ParkingLocationEarlyBirdOffer earlyBirdOffer, DateTime StartDate, string StartTime, string EndTime)
        {

            string BookingDay = StartDate.DayOfWeek.ToString();

            if (earlyBirdOffer != null && TimeSpan.Parse(StartTime) >= TimeSpan.Parse(earlyBirdOffer.EnterFromTime) && TimeSpan.Parse(StartTime) <= TimeSpan.Parse(earlyBirdOffer.EnterToTime) && TimeSpan.Parse(EndTime) <= TimeSpan.Parse(earlyBirdOffer.ExitByTime))
            {
                bool checkBookingDay = (earlyBirdOffer.IsMonday && BookingDay.Equals(EDays.Monday.ToString())) || (earlyBirdOffer.IsTuesday && BookingDay.Equals(EDays.Tuesday.ToString())) || (earlyBirdOffer.IsWednesday && BookingDay.Equals(EDays.Wednesday.ToString())) || (earlyBirdOffer.IsThursday && BookingDay.Equals(EDays.Thursday.ToString())) || (earlyBirdOffer.IsFriday && BookingDay.Equals(EDays.Friday.ToString())) || (earlyBirdOffer.IsSaturday && BookingDay.Equals(EDays.Saturday.ToString())) || (earlyBirdOffer.IsSunday && BookingDay.Equals(EDays.Sunday.ToString()));

                if (checkBookingDay)
                {

                    EarlyBirdInfo earlyBirdInfo = new EarlyBirdInfo
                    {
                        EarlyBirdId = earlyBirdOffer.Id,
                        Amount = earlyBirdOffer.Amount,
                        EnterFromTime = earlyBirdOffer.EnterFromTime,
                        ExitByTime = earlyBirdOffer.ExitByTime,
                        EnterToTime = earlyBirdOffer.EnterToTime
                    };
                    return earlyBirdInfo;
                }

            }
            return null;
        }

        public NightFareInfo GetNightFareInfo(ParkingLocationNightFareOffer nightFareOffer, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime)
        {
            string BookingDay = StartDate.DayOfWeek.ToString();
            if (nightFareOffer != null) 
            {
                TimeSpan startTime = TimeSpan.Parse(StartTime);
                TimeSpan endTime = TimeSpan.Parse(EndTime);
                TimeSpan enterFromTime = TimeSpan.Parse(nightFareOffer.EnterFromTime);
                TimeSpan enterToTime = TimeSpan.Parse(nightFareOffer.EnterToTime);
                TimeSpan exitByTime = TimeSpan.Parse(nightFareOffer.ExitByTime);

                DateTime bookingStartTime = StartDate.Date.Add(startTime);
                DateTime bookingEndTime = EndDate.Date.Add(endTime);
                DateTime offerEnterFromDateTime = StartDate.Date.Add(enterFromTime);

                TimeSpan enterTodifference = enterToTime > enterFromTime
                    ? enterToTime - enterFromTime
                    : enterToTime + TimeSpan.FromHours(24) - enterFromTime;

                TimeSpan exitFromDifference = exitByTime > enterFromTime
                    ? exitByTime - enterFromTime
                    : exitByTime + TimeSpan.FromHours(24) - enterFromTime;

                DateTime offerEnterToDateTime = offerEnterFromDateTime.Add(enterTodifference);
                DateTime offerExitByDateTime = offerEnterFromDateTime.Add(exitFromDifference);


                if (bookingStartTime >= offerEnterFromDateTime &&
                    bookingStartTime <= offerEnterToDateTime &&
                    bookingEndTime <= offerExitByDateTime &&
                    ((nightFareOffer.IsMonday && BookingDay.Equals(EDays.Monday.ToString())) ||
                        (nightFareOffer.IsTuesday && BookingDay.Equals(EDays.Tuesday.ToString())) ||
                        (nightFareOffer.IsWednesday && BookingDay.Equals(EDays.Wednesday.ToString())) ||
                        (nightFareOffer.IsThursday && BookingDay.Equals(EDays.Thursday.ToString())) ||
                        (nightFareOffer.IsFriday && BookingDay.Equals(EDays.Friday.ToString())) ||
                        (nightFareOffer.IsSaturday && BookingDay.Equals(EDays.Saturday.ToString())) ||
                        (nightFareOffer.IsSunday && BookingDay.Equals(EDays.Sunday.ToString()))))
                {
                    NightFareInfo nightFareInfo = new NightFareInfo
                    {
                        NightFareId = nightFareOffer.Id,
                        Amount = nightFareOffer.Amount,
                        EnterFromTime = nightFareOffer.EnterFromTime,
                        ExitByTime = nightFareOffer.ExitByTime,
                        EnterToTime = nightFareOffer.EnterToTime
                    };
                    return nightFareInfo;
                }
            }

            return null;
        }



        public TimeSpan ConvertToUtc(DateTime date, TimeSpan time, TimeZoneInfo timeZoneInfo)
        {
            DateTime dateStart = DateTime.Parse(date.ToShortDateString()) + time;
            DateTime dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateStart, timeZoneInfo);
            return dateUtc.TimeOfDay;
        }

        public SearchParkingSlots GetSlotObject(SearchParkingSlots objSlot, DateTime Date, TimeZoneInfo timeZoneInfo, TimeSpan StartTime, TimeSpan EndTime, bool IsDayChanged, ref double Duration)
        {
            DateTime dtStart, dtEnd;
            dtStart = Date + StartTime;
            dtEnd = Date + EndTime;
            objSlot.WeekDay = Date.DayOfWeek.ToString();
            objSlot.IsMonday = objSlot.WeekDay.Equals(EDays.Monday.ToString()) ? true : false;
            objSlot.IsTuesday = objSlot.WeekDay.Equals(EDays.Tuesday.ToString()) ? true : false;
            objSlot.IsWednesday = objSlot.WeekDay.Equals(EDays.Wednesday.ToString()) ? true : false;
            objSlot.IsThursday = objSlot.WeekDay.Equals(EDays.Thursday.ToString()) ? true : false;
            objSlot.IsFriday = objSlot.WeekDay.Equals(EDays.Friday.ToString()) ? true : false;
            objSlot.IsSaturday = objSlot.WeekDay.Equals(EDays.Saturday.ToString()) ? true : false;
            objSlot.IsSunday = objSlot.WeekDay.Equals(EDays.Sunday.ToString()) ? true : false;

            objSlot.StartDate = dtStart;
            objSlot.EndDate = dtEnd;
            DateTime StartDateUtc = TimeZoneInfo.ConvertTimeToUtc(dtStart, timeZoneInfo);
            DateTime EndDateUtc = TimeZoneInfo.ConvertTimeToUtc(dtEnd, timeZoneInfo);
            objSlot.StartDateUtc = StartDateUtc;
            objSlot.EndDateUtc = EndDateUtc;
            objSlot.StartTime = StartTime;
            objSlot.StartTimeUtc = StartDateUtc.TimeOfDay;
            objSlot.EndTime = EndTime;
            objSlot.EndTimeUtc = (EndTime.Equals(TimeSpan.Parse("23:59:59")) ? EndDateUtc.TimeOfDay : EndDateUtc.AddSeconds(-1).TimeOfDay);

            Duration = CalculateDuration(Date, StartTime, EndTime);

            return objSlot;
        }
        public List<ParkingLocationRate> GetParkingLocationRates(List<List<ParkingLocationRateRequest>> model)
        {
            List<ParkingLocationRate> lstRates = new List<ParkingLocationRate>();
            ParkingLocationRate rates = new ParkingLocationRate();

            for (int i = 0; i < model.Count; i++)
            {
                foreach (var rate in model[i].ToList())
                {
                    rates = new ParkingLocationRate();
                    switch (i)
                    {
                        case 0:
                            rates.BookingType = "Hourly";
                            break;

                        case 1:
                            rates.BookingType = "Monthly";
                            break;
                    }

                    rates.Charges = rate.Charges;
                    rates.Duration = rate.Duration;
                    lstRates.Add(rates);
                }
            }

            return lstRates;
        }

        public (double, List<BookingDetails>) GetBookingDetails(BookingRequest model, TimeZoneInfo timeZoneInfo)
        {
            double TotalHours = 0.00;
            List<BookingDetails> bookingDetailList = new List<BookingDetails>();

            TimeSpan StartTimeSpan, EndTimeSpan;
            bool IsDayChanged = false;

            StartTimeSpan = TimeSpan.Parse(model.StartTime);
            EndTimeSpan = TimeSpan.Parse(model.EndTime);


            if (EndTimeSpan < StartTimeSpan) IsDayChanged = true;


            List<DateTime> SearchDates = GetDateList(model.StartDate, model.EndDate);

            BookingDetails objBooking;
            foreach (DateTime Date in SearchDates)
            {

                if (model.IsFullTimeBooking)
                {
                    objBooking = CreateBookingDetailObject(Date, timeZoneInfo, Date == model.StartDate ? StartTimeSpan : TimeSpan.Parse("00:00:00"), Date == model.EndDate ? EndTimeSpan : TimeSpan.Parse("23:59:59"), IsDayChanged, model.PerHourRate);
                    TotalHours += objBooking.Duration;
                    bookingDetailList.Add(objBooking);
                }
                else
                {

                    #region old lengthy code, not in use   // if (IsDayChanged)
                    // {

                    //     if (Date != model.EndDate)
                    //     {
                    //         objBooking = CreateBookingDetailObject(Date, timeZoneInfo, StartTimeSpan, TimeSpan.Parse("23:59:59"), IsDayChanged, model.PerHourRate);
                    //         TotalHours += objBooking.Duration;
                    //         bookingDetailList.Add(objBooking);
                    //     }
                    //     if (Date != model.StartDate)
                    //     {
                    //         objBooking = CreateBookingDetailObject(Date, timeZoneInfo, TimeSpan.Parse("00:00:00"), EndTimeSpan, IsDayChanged, model.PerHourRate);
                    //         TotalHours += objBooking.Duration;
                    //         bookingDetailList.Add(objBooking);
                    //     }
                    // }
                    // else
                    // {
                    #endregion
                    objBooking = CreateBookingDetailObject(Date, timeZoneInfo, StartTimeSpan, EndTimeSpan, IsDayChanged, model.PerHourRate);
                    TotalHours += objBooking.Duration;
                    bookingDetailList.Add(objBooking);
                    //  }
                }
            }



            return (TotalHours, bookingDetailList);
        }

        public BookingDetails CreateBookingDetailObject(DateTime Date, TimeZoneInfo timeZoneInfo, TimeSpan StartTime, TimeSpan EndTime, bool IsDayChanged, decimal PerHourRate, decimal Duration = 0.00m, decimal Charges = 0.00m, bool IsfromOverStayCalculation = false)
        {

            BookingDetails objBooking = new BookingDetails();
            DateTime dtStart, dtEnd;
            dtStart = Date + StartTime;
            dtEnd = Date + EndTime;

            DateTime EndDateUtc = TimeZoneInfo.ConvertTimeToUtc(dtEnd, timeZoneInfo);
            EndDateUtc = (EndTime.Equals(TimeSpan.Parse("23:59:59")) ? EndDateUtc : EndDateUtc.AddSeconds(-1));

            //deduction of duration will take place only when calculating overstay charges.
            objBooking.Duration = CalculateDuration(Date, StartTime, EndTime) - Convert.ToDouble(Duration);
            objBooking.Duration = objBooking.Duration < 0 ? 0 : objBooking.Duration;
            objBooking.StartDate = Date;
            objBooking.EndDate = Date;
            objBooking.StartDateUtc = TimeZoneInfo.ConvertTimeToUtc(dtStart, timeZoneInfo);
            objBooking.EndDateUtc = EndDateUtc;
            objBooking.StartTime = StartTime;
            objBooking.EndTime = EndTime;

            //charges are 0.00 only when calculating overstay charges. They are adjusted later in the first record of overstay booking list. 
            objBooking.Charges = IsfromOverStayCalculation ? 0.00m : Convert.ToDecimal(objBooking.Duration * Convert.ToDouble(PerHourRate));
            return objBooking;
        }

        public (decimal, decimal, ParkingLocationRateRequest) GetTotalHoursandAmountByDuration(List<ParkingLocationRateRequest> rates, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, bool IsFullTimeBooking, bool IsfromOverStayCalculation = false)
        {
            ParkingLocationRateRequest rate; decimal PerHourRate;
            rates = rates.OrderBy(a => a.Duration).ToList();
            double TotalHours = 0.00;
            TimeSpan StartTimeSpan, EndTimeSpan;
            // bool IsDayChanged = false;

            StartTimeSpan = TimeSpan.Parse(StartTime);
            EndTimeSpan = TimeSpan.Parse(EndTime);


            //  if (EndTimeSpan < StartTimeSpan) IsDayChanged = true;

            double FetchTotalHours = 0.00;

            if (IsFullTimeBooking)
                FetchTotalHours = ((EndDate + EndTimeSpan) - (StartDate + StartTimeSpan)).TotalHours;
            else
                FetchTotalHours = ((StartDate + EndTimeSpan) - (StartDate + StartTimeSpan)).TotalHours;


            TotalHours = IsFullTimeBooking ? FetchTotalHours : (IsfromOverStayCalculation ? FetchTotalHours : FetchTotalHours * 30);


            #region not needed, old code
            // List<DateTime> SearchDates = GetDateList(StartDate, EndDate);
            // double Duration = 0.00;
            // foreach (DateTime Date in SearchDates)
            // {

            //     if (IsFullTimeBooking)
            //     {
            //         Duration = CalculateDuration(Date, Date == StartDate ? StartTimeSpan : TimeSpan.Parse("00:00:00"), Date == EndDate ? EndTimeSpan : TimeSpan.Parse("23:59:59"));
            //         TotalHours += Duration;
            //     }
            //     else
            //     {

            //         if (IsDayChanged)
            //         {

            //             if (Date != EndDate)
            //             {
            //                 Duration = CalculateDuration(Date, StartTimeSpan, TimeSpan.Parse("23:59:59"));
            //                 TotalHours += Duration;
            //             }
            //             if (Date != StartDate)
            //             {
            //                 Duration = CalculateDuration(Date, TimeSpan.Parse("00:00:00"), EndTimeSpan);

            //                 TotalHours += Duration;
            //             }
            //         }
            //         else
            //         {

            //             Duration = CalculateDuration(Date, StartTimeSpan, EndTimeSpan);
            //             TotalHours += Duration;
            //         }
            //     }
            // }

            // double FetchTotalHours = TotalHours;


            // if (!IsFullTimeBooking)
            // {
            //     TimeSpan ts = GetMonthlyBookingTimeDifference(StartDate, StartTimeSpan, EndTimeSpan);
            //     FetchTotalHours = ts.TotalHours;
            // }

            #endregion

            var (duration, charges) = FetchMaxDurationandCharges(rates, FetchTotalHours);
            // (rate, IsFinalAmount) = GetTotalAmountByLocationRates(rates, ts.TotalHours);
            PerHourRate = Convert.ToDecimal(Convert.ToDouble(charges) / FetchTotalHours);

            rate = new ParkingLocationRateRequest
            {
                Duration = Convert.ToInt32(duration),
                Charges = charges
            };

            return (Convert.ToDecimal(TotalHours), PerHourRate, rate);

        }

        public StatesMst GetState(List<StatesMst> ListStates, long Id, ref StatesMst state)
        {
            if (state != null && state.Id == Id) return state;
            else
            {
                state = ListStates.Find(a => a.Id == Id);
                return state;
            }

        }

        public Countries GetCountry(List<Countries> ListCountries, long Id, ref Countries country)
        {
            if (country != null && country.Id == Id) return country;
            else
            {
                return ListCountries.Find(a => a.Id == Id);
            }
        }


        public (double, decimal) FetchMaxDurationandCharges(List<ParkingLocationRateRequest> rates, double TotalHours)
        {
            double Duration = 0.00; decimal Charges = 0.00m;
            var rate = rates.Find(e => e.Duration >= TotalHours);
            if (rate != null)
            {
                Duration = rate.Duration;
                Charges = rate.Charges;
                return (rate.Duration, rate.Charges);
            }
            else
            {
                rate = rates.First(a => a.Duration.Equals(rates.Max(a => a.Duration)));
                Duration = rate.Duration;
                Charges = rate.Charges;
                var (CDuration, CCharges) = FetchMaxDurationandCharges(rates, (TotalHours - Convert.ToDouble(Duration)));
                Duration += CDuration;
                Charges += CCharges;
                return (Duration, Charges);

            }
        }


        public TimeSpan GetMonthlyBookingTimeDifference(DateTime StartDate, TimeSpan StartTime, TimeSpan EndTime)
        {
            DateTime DurationDate, DurationEndDate;
            DurationDate = StartDate + StartTime;

            if (EndTime < StartTime)
                DurationEndDate = StartDate.AddDays(1) + EndTime;
            else
                DurationEndDate = StartDate + EndTime;

            return DurationEndDate - DurationDate;
        }

        public (ParkingLocationRateRequest, bool) GetTotalAmountByLocationRates(List<ParkingLocationRateRequest> rates, double TotalHours)
        {

            var rate = rates.Find(e => e.Duration >= TotalHours);
            if (rate != null)
            {
                return (rate, true);

            }
            else
            {
                rate = rates.First(a => a.Duration.Equals(rates.Max(a => a.Duration)));
                return (rate, false);
            }

        }
        public double CalculateDuration(DateTime Date, TimeSpan StartTime, TimeSpan EndTime)
        {
            DateTime dtStart, dtEnd;
            dtStart = Date + StartTime;
            dtEnd = Date + EndTime;

            if (dtEnd.TimeOfDay.Equals(TimeSpan.Parse("23:59:59"))) dtEnd = dtEnd.AddSeconds(1);
            TimeSpan hourDifference = dtEnd - dtStart;
            return hourDifference.TotalHours;
        }

        public double CalculateDuration(DateTime StartDate, DateTime EndDate, TimeSpan StartTime, TimeSpan EndTime)
        {
            DateTime dtStart, dtEnd;
            dtStart = StartDate + StartTime;
            dtEnd = EndDate + EndTime;

            if (dtEnd.TimeOfDay.Equals(TimeSpan.Parse("23:59:59"))) dtEnd = dtEnd.AddSeconds(1);
            TimeSpan hourDifference = dtEnd - dtStart;
            return hourDifference.TotalHours;
        }

        public decimal RoundOff(dynamic value)
        {
            return Math.Round(Convert.ToDecimal(value), 2, MidpointRounding.AwayFromZero);
        }

        public double DoubleRoundOff(dynamic value)
        {
            return Math.Round(Convert.ToDouble(value), 2, MidpointRounding.AwayFromZero);
        }

        public string[] GetMobileSplitedValues(string Mobile)
        {
            string[] SplittedMobileValues = new string[2];
            if (!string.IsNullOrEmpty(Mobile))
            {
                if (Mobile.StartsWith("+1"))
                {
                    SplittedMobileValues[0] = "+1";
                    SplittedMobileValues[1] = Mobile.Substring(2, Mobile.Length - 2);
                }
                else if (Mobile.StartsWith("+91"))
                {
                    SplittedMobileValues[0] = "+91";
                    SplittedMobileValues[1] = Mobile.Substring(3, Mobile.Length - 3);
                }
                else
                {
                    SplittedMobileValues[0] = "";
                    SplittedMobileValues[1] = Mobile;
                }
            }

            return SplittedMobileValues;

        }


        public string GetMobileWithoutSpecialCharacter(string Mobile)
        {

            return string.IsNullOrEmpty(Mobile) ? null : Regex.Replace(Mobile, @"[^+0-9]+", "");
        }

        internal (object, List<object>) GetBookingDetailsv1(BookingFromQrRequest model, TimeZoneInfo timeZoneInfo)
        {
            throw new NotImplementedException();
        }
    }
}