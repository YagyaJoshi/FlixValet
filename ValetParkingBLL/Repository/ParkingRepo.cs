using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models.ParkingLocationModels;
using System.Linq;
using System.Collections.Generic;
using ValetParkingDAL.Models;
using System;
using System.Data;
using ValetParkingDAL.Enums;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingAPI.Models;
using AutoMapper;
using ValetParkingDAL.Models.PaymentModels.cs;
using System.Text.Json;
using ValetParkingDAL.Models.UserModels;
using ValetParkingDAL.Models.StateModels;
using ValetParkingDAL.Models.QRModels;
using BC = BCrypt.Net.BCrypt;
using VehicleDetail = ValetParkingDAL.Models.NumberPlateRecogModels;
using MailKit.Search;
using Twilio.Base;
using System.IO;

namespace ValetParkingBLL.Repository
{
	public class ParkingRepo : IParking
	{
		private readonly IConfiguration _configuration;
		private readonly DateTimeHelper dateTimeHelper;
		private readonly ICache _cacheRepo;
		private readonly IQR _qRRepo;
		private readonly IMapper _mapper;
		private readonly ParkingHelper parkingHelper;
		private readonly AppSettings _appsettings;
		private readonly IAWSQueueService _aWSQueueService;
		private readonly AWSSQSDetails _aWSSQSDetails;
        private readonly IEmail _emailService;


		public ParkingRepo(IMapper mapper, IConfiguration configuration, ParkingHelper ParkingHelper, DateTimeHelper DateTimeHelper, ICache cacheRepo, IQR qRRepo, IAWSQueueService aWSQueueService)
		{
			_configuration = configuration;
			parkingHelper = ParkingHelper;
			dateTimeHelper = DateTimeHelper;
			_cacheRepo = cacheRepo;
			_qRRepo = qRRepo;
			_mapper = mapper;
			_aWSQueueService = aWSQueueService;
			_appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
			_aWSSQSDetails = _configuration.GetSection("AWSSQSDetails").Get<AWSSQSDetails>();
			var config = new MapperConfiguration(cfg =>
		 {
			 cfg.CreateMap<GuestPreBookingRequest, CurrentLocationRequest>();
			 cfg.CreateMap<GuestPreBookingRequest, GuestPrebookingResponse>();
			 cfg.CreateMap<RequestParkingRates, ParkingLocationRateRequest>();
		 });
			_mapper = config.CreateMapper();

		}
        public ParkingRepo(IMapper mapper, IConfiguration configuration, ParkingHelper ParkingHelper, DateTimeHelper DateTimeHelper, ICache cacheRepo, IQR qRRepo, IAWSQueueService aWSQueueService, IEmail emailService)
        {
            _configuration = configuration;
            parkingHelper = ParkingHelper;
            dateTimeHelper = DateTimeHelper;
            _cacheRepo = cacheRepo;
            _qRRepo = qRRepo;
            _mapper = mapper;
            _aWSQueueService = aWSQueueService;
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _aWSSQSDetails = _configuration.GetSection("AWSSQSDetails").Get<AWSSQSDetails>();
            var config = new MapperConfiguration(cfg =>
         {
             cfg.CreateMap<GuestPreBookingRequest, CurrentLocationRequest>();
             cfg.CreateMap<GuestPreBookingRequest, GuestPrebookingResponse>();
             cfg.CreateMap<RequestParkingRates, ParkingLocationRateRequest>();
         });
            _mapper = config.CreateMapper();
            _emailService = emailService;
        }

        public long AddParkingLocation(ParkingLocationRequest model)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddParkingLocationDetails_v2");
			try
			{
                objCmd.Parameters.AddWithValue("@ParkingLocationId", model.Id);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@Address", model.Address);
				objCmd.Parameters.AddWithValue("@City", model.City);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@ZipCode", model.ZipCode);
				objCmd.Parameters.AddWithValue("@LocationPic", model.LocationPic);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@Latitude", model.Latitude);
				objCmd.Parameters.AddWithValue("@Longitude", model.Longitude);
				objCmd.Parameters.AddWithValue("@NoofSpaces", model.No_of_Spaces);
				objCmd.Parameters.AddWithValue("@Currency", model.Currency);
				objCmd.Parameters.AddWithValue("@TimeZone", model.TimeZoneId);
				objCmd.Parameters.AddWithValue("@Instructions", model.Instructions);
				objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
				objCmd.Parameters.AddWithValue("@LocationName", model.LocationName);
				objCmd.Parameters.AddWithValue("@OverSizedChargesRegular", model.OverSizedChargesRegular);
				objCmd.Parameters.AddWithValue("@OverSizedChargesMonthly", model.OverSizedChargesMonthly);
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@Tax", model.Tax);
				objCmd.Parameters.AddWithValue("@ConvenienceFee", model.ConvenienceFee);
				objCmd.Parameters.AddWithValue("@ParkingTimings", MapDataTable.ToDataTable(parkingHelper.GetParkingTimingList(model.ParkingTimings, model.StartDate, model.TimeZone)));
				objCmd.Parameters.AddWithValue("@ParkingRates", MapDataTable.ToDataTable(parkingHelper.GetParkingLocationRates(model.ParkingRates)));
                objCmd.Parameters.AddWithValue("@IsMonthlySubscription", model.IsMonthlySubscription);
                objCmd.Parameters.AddWithValue("@PricingPlanId", model.PricingPlanId);


                model.EarlyBirdOffer.ForEach(a => a.BookingType = "Hourly");

				model.EarlyBirdOffer.ForEach(a => a.BookingType = "Hourly");
                model.EarlyBirdOffer.ForEach(a => a.IsActive = true);


                if (model.Id > 0 && model.EarlyBirdOffer != null && model.EarlyBirdOffer.Count > 0)
					model.EarlyBirdOffer.ForEach(a => a.ParkingLocationId = model.Id);
				objCmd.Parameters.AddWithValue("@ParkingEarlyBirdOffer", MapDataTable.ToDataTable(model.EarlyBirdOffer));

                model.NightFareOffer.ForEach(a => a.BookingType = "Hourly");
                model.NightFareOffer.ForEach(a => a.IsActive = true);
                if (model.Id > 0 && model.NightFareOffer != null && model.NightFareOffer.Count > 0)
                    model.NightFareOffer.ForEach(a => a.ParkingLocationId = model.Id);
                objCmd.Parameters.AddWithValue("@ParkingNightFareOffer", MapDataTable.ToDataTable(model.NightFareOffer));
                DataTable dtLoc = objSQL.FetchDT(objCmd);

				if (!string.IsNullOrEmpty(Convert.ToString(dtLoc.Rows[0]["Error"])))
					throw new AppException(Convert.ToString(dtLoc.Rows[0]["Error"]));

				return Convert.ToInt64(dtLoc.Rows[0]["Id"]);
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


		public ParkingLocationRequest GetParkingLocationDetails(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", Id);
				DataSet ds = objSQL.FetchDB(objCmd);

				// var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
				// if (!string.IsNullOrEmpty(Error))
				//     throw new AppException(Error);


				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				if (ds.Tables[1].Rows.Count > 0)
				{
					var plocation = (from DataRow dr in ds.Tables[1].Rows
									 select new ParkingLocationRequest
									 {
										 Id = Convert.ToInt64(dr["Id"]),
										 UserId = Convert.ToInt64(dr["UserId"]),
										 Address = dr["Address"].ToString(),
										 City = dr["City"].ToString(),
										 State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
										 StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
										 Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
										 CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
										 ZipCode = dr["ZipCode"].ToString(),
										 LocationPic = dr["LocationPic"].ToString(),
										 Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
										 MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
										 Latitude = Convert.ToString(dr["Latitude"]),
										 Longitude = Convert.ToString(dr["Longitude"]),
										 No_of_Spaces = Convert.ToInt32(dr["No_of_Spaces"].ToString()),
										 Currency = dr["Currency"].ToString(),
										 TimeZone = dr["TimeZone"].ToString(),
										 Instructions = dr["Instructions"].ToString(),
										 IsActive = Convert.ToBoolean(dr["IsActive"]),
										 CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
										 UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"]),
										 LocationName = dr["LocationName"].ToString(),
										 Tax = Convert.ToDecimal(dr["Tax"]),
										 OverSizedChargesRegular = Convert.ToDecimal(dr["OverSizedChargesRegular"]),
										 OverSizedChargesMonthly = Convert.ToDecimal(dr["OverSizedChargesMonthly"]),
										 ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
										 ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
										 IsMonthlySubscription = Convert.ToBoolean(dr["IsMonthlySubscription"]),
										 PricingPlanId = Convert.ToInt64(dr["PricingPlanId"])
									 }).FirstOrDefault();

					var timings = (from DataRow dr in ds.Tables[2].Rows
								   select new ParkingLocationTiming
								   {
									   IsMonday = Convert.ToBoolean(dr["IsMonday"]),
									   IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
									   IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
									   IsThursday = Convert.ToBoolean(dr["IsThursday"]),
									   IsFriday = Convert.ToBoolean(dr["IsFriday"]),
									   IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
									   IsSunday = Convert.ToBoolean(dr["IsSunday"]),
									   StartDate = Convert.ToDateTime(dr["StartDate"]),
									   StartDateUtc = Convert.ToDateTime(dr["StartDateUtc"]),
									   StartTime = TimeSpan.Parse(dr["StartTime"].ToString()),
									   EndTime = TimeSpan.Parse(dr["EndTime"].ToString())
								   }).ToList();
					plocation.StartDate = timings.Count > 0 ? Convert.ToDateTime(timings.Select(a => a.StartDate).FirstOrDefault()) : DateTime.MinValue;
					plocation.ParkingTimings = parkingHelper.GetTimings(timings);

					var rates = (from DataRow dr in ds.Tables[3].Rows
								 select new ParkingLocationRate
								 {
									 BookingType = dr["BookingType"].ToString(),
									 Duration = Convert.ToInt32(dr["Duration"]),
									 Charges = Convert.ToDecimal(dr["Charges"])
								 }).ToList();

					plocation.ParkingRates = parkingHelper.GetRates(rates);


					var earlyBirdOffer = (from DataRow dr in ds.Tables[4].Rows
										  select new ParkingLocationEarlyBirdOffer
										  {
											  Id = Convert.ToInt64(dr["Id"]),
											  IsMonday = Convert.ToBoolean(dr["IsMonday"]),
											  IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
											  IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
											  IsThursday = Convert.ToBoolean(dr["IsThursday"]),
											  IsFriday = Convert.ToBoolean(dr["IsFriday"]),
											  IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
											  IsSunday = Convert.ToBoolean(dr["IsSunday"]),
											  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											  Amount = Convert.ToDecimal(dr["Amount"]),
											  BookingType = Convert.ToString(dr["BookingType"]),
											  EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
											  EnterToTime = Convert.ToString(dr["EnterToTime"]),
											  ExitByTime = Convert.ToString(dr["ExitByTime"]),
											  IsActive = Convert.ToBoolean(dr["IsActive"])
										  }).ToList();

                    var nightFareOffer = (from DataRow dr in ds.Tables[5].Rows
                                          select new ParkingLocationNightFareOffer
                                          {
                                              Id = Convert.ToInt64(dr["Id"]),
                                              IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                              IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                              IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                              IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                              IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                              IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                              IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                              ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                              Amount = Convert.ToDecimal(dr["Amount"]),
                                              BookingType = Convert.ToString(dr["BookingType"]),
                                              EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
                                              EnterToTime = Convert.ToString(dr["EnterToTime"]),
                                              ExitByTime = Convert.ToString(dr["ExitByTime"]),
                                              IsActive = Convert.ToBoolean(dr["IsActive"])
                                          }).ToList();

                    plocation.EarlyBirdOffer = earlyBirdOffer;
					plocation.NightFareOffer = nightFareOffer;
					return plocation;


				}
				else return null;
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


		public ParkingLocationsResponse GetAllParkingLocations(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetAllParkingLocations");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@LocationsList", LocationsList);

				DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new LocationsList
										{
											Id = Convert.ToInt64(dr["Id"]),
											Address = Convert.ToString(dr["Address"]),
											City = Convert.ToString(dr["City"]),
											State = Convert.ToString(dr["State"]),
											Country = Convert.ToString(dr["Country"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											No_of_Spaces = Convert.ToInt32(dr["No_of_Spaces"])
										}).ToList();

					return new ParkingLocationsResponse { ParkingLocations = lstlocations, Total = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]) };
				}
				return null;
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

		public List<ParkingLocationName> GetParkingLocationsByOwner(long ParkingBusinessOwnerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetLocationsByUser");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				//  sp_GetLocationsByUser
				DataTable dtLocations = objSQL.FetchDT(objCmd);
				var lstlocations = (from DataRow dr in dtLocations.Rows
									select new ParkingLocationName
									{
										Id = Convert.ToInt64(dr["Id"]),
										LocationName = Convert.ToString(dr["LocationName"])
									}).ToList();
				return lstlocations;
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

		public long DeleteParkingLocation(ParkingLocationIdModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_DeleteLocation");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				DataTable dtLoc = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtLoc.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.ParkingLocationId;
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
		public List<RequestParkingLocations> SearchParkingLocations(CurrentLocationRequest model)
		{
			// List<SearchParkingSlots> SearchList = new List<SearchParkingSlots>();
			// double TotalHours=0.00;
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_SearchParkingLocations");
			try
			{
				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());
				string WeekDay = model.StartDate.DayOfWeek.ToString();


				DateTime StartdateUtc = TimeZoneInfo.ConvertTimeToUtc(model.StartDate, timeZoneInfo);

				var (TotalHours, SearchList) = parkingHelper.GetSearchDateTimingwiseTable(model, timeZoneInfo);
				objCmd.Parameters.AddWithValue("@Latitude", model.Latitude);
				objCmd.Parameters.AddWithValue("@Longitude", model.Longitude);
				objCmd.Parameters.AddWithValue("@RDistance", model.RDistance);
				objCmd.Parameters.AddWithValue("@StartDate", model.StartDate);
				objCmd.Parameters.AddWithValue("@EndDate", model.EndDate == null ? (DateTime?)null : Convert.ToDateTime(model.EndDate));
				objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);

				objCmd.Parameters.AddWithValue("@SearchParkingSlots", MapDataTable.ToDataTable(SearchList));


				DataSet ds = objSQL.FetchDB(objCmd);
				List<RequestParkingRates> rates = new List<RequestParkingRates>();

				if (ds.Tables[0].Rows.Count > 0)
				{

					rates = (from DataRow dr in ds.Tables[1].Rows
							 select new RequestParkingRates
							 {
								 ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
								 BookingType = Convert.ToString(dr["BookingType"]),
								 Duration = model.BookingType.ToLower() == "monthly" ? Convert.ToInt32(dr["Duration"]) * 30 : Convert.ToInt32(dr["Duration"]),
								 Charges = Convert.ToDecimal(dr["Charges"])
							 }).ToList();

					var locations = (from DataRow dr in ds.Tables[0].Rows
									 select new RequestParkingLocations
									 {
										 Id = Convert.ToInt64(dr["Id"]),
										 LocationName = Convert.ToString(dr["LocationName"]),
										 Address = Convert.ToString(dr["Address"]),
										 City = Convert.ToString(dr["City"]),
										 Country = Convert.ToString(dr["Country"]),
										 Latitude = Convert.ToString(dr["Latitude"]),
										 Longitude = Convert.ToString(dr["Longitude"]),
										 ZipCode = Convert.ToString(dr["ZipCode"]),
										 LocationPic = Convert.ToString(dr["LocationPic"]),
										 No_of_Spaces = Convert.ToInt32(dr["No_of_Spaces"]),
										 ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
										 Rates = rates.Count > 0 ? rates.Where(a => a.ParkingLocationId.Equals(Convert.ToInt64(dr["Id"]))).Select(a => a).ToList() : null,
										 Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
										 MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
										 Distance = Convert.ToDecimal(dr["Distance"]),
										 Currency = Convert.ToString(dr["Currency"]),
										 Symbol = Convert.ToString(dr["Symbol"])
									 }).ToList();

                    double TotalDuration = 0.00;
					if (model.IsFullTimeBooking)
						TotalDuration = ((model.EndDate + TimeSpan.Parse(model.EndTime)) - (model.StartDate + TimeSpan.Parse(model.StartTime))).TotalHours;
					else
						TotalDuration = ((model.StartDate + TimeSpan.Parse(model.EndTime)) - (model.StartDate + TimeSpan.Parse(model.StartTime))).TotalHours;


					foreach (var item in locations)
					{
						item.TotalHours = model.IsFullTimeBooking ? model.BookingType.ToLower() == "monthly" ? Math.Round(TotalDuration): TotalDuration : TotalDuration * 30;
						var Charges = parkingHelper.FetchMaxDurationandCharges(_mapper.Map<List<ParkingLocationRateRequest>>(rates.Where(e => e.BookingType.ToLower() == model.BookingType.ToLower() && e.ParkingLocationId == item.Id).ToList()), TotalDuration).Item2;
						item.TotalCharges = model.IsFullTimeBooking ? Charges : Charges * 30;
					}

					return locations;
				}
				else
					throw new AppException("No locations found!");
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

		public BookingIdResponse BookParkingLocation(BookingRequest model, object PaymentInfo = null)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_BookParkingLocation");
			try
			{
				TimeSpan StartTime = TimeSpan.Parse(model.StartTime), EndTime = TimeSpan.Parse(model.EndTime), perDayDurationSpan = new TimeSpan();
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());

				//calculating per day hour difference not considered in case of full time booking
				if (!model.IsFullTimeBooking)
				{
					perDayDurationSpan = parkingHelper.GetMonthlyBookingTimeDifference(model.StartDate, StartTime, EndTime);
				}

				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);

				var (TotalHours, lstBookingDetails) = parkingHelper.GetBookingDetails(model, timeZoneInfo);

				objSQL = new SQLManager(_configuration);
				if (model.IsEarlyBirdOfferApplied)
				{


					var EBhours = parkingHelper.CalculateDuration(model.StartDate.Date, TimeSpan.Parse(model.StartTime), TimeSpan.Parse(model.EndTime));
					model.MaxDurationofSlab = Convert.ToDouble(EBhours);
					model.MaxRateofSlab = model.EarlyBirdAmount;
				}


				model.OverSizedCharges = model.OverSizedCharges ?? 0.00m;
				objCmd.Parameters.AddWithValue("@Id", model.Id);
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);
				objCmd.Parameters.AddWithValue("@PerDayDuration", model.IsFullTimeBooking ? TotalHours : perDayDurationSpan.TotalHours);
				objCmd.Parameters.AddWithValue("@TotalDuration", TotalHours);
				objCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@IsEarlyBirdOfferApplied", model.IsEarlyBirdOfferApplied);
				objCmd.Parameters.AddWithValue("@EarlyBirdId", model.EarlyBirdId);
				objCmd.Parameters.AddWithValue("@EarlyBirdAmount", model.EarlyBirdAmount);
				objCmd.Parameters.AddWithValue("@OverSizedCharges", model.OverSizedCharges);
				objCmd.Parameters.AddWithValue("@TaxAmount", model.IsEarlyBirdOfferApplied ? model.EarlyBirdTaxAmount : model.TaxAmount);
				objCmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
				objCmd.Parameters.AddWithValue("@FinalAmount", model.IsEarlyBirdOfferApplied ? model.EarlyBirdFinalAmount : model.FinalAmount);
				objCmd.Parameters.AddWithValue("@IsPaymentFromCustomerSite", model.IsPaymentFromCustomerSite);
				objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);


				var Notes = (model.StartDate + StartTime).ToString("MMM dd h:mm tt") + " to " + (model.EndDate + EndTime).ToString("MMM dd h:mm tt");
				if (PaymentInfo != null)
				{
					if (PaymentInfo is StripeChargesApiResponse)
					{
						StripeChargesApiResponse paymentModel = (StripeChargesApiResponse)PaymentInfo;
						objCmd.Parameters.AddWithValue("@TransactionId", paymentModel.id);
						objCmd.Parameters.AddWithValue("@TransactionMethod", paymentModel.card.funding);
						objCmd.Parameters.AddWithValue("@ProcessResponseCode", paymentModel.balance_transaction);
						objCmd.Parameters.AddWithValue("@ProcessResponseText", paymentModel.outcome == null ? null : paymentModel.outcome.seller_message);
						objCmd.Parameters.AddWithValue("@TransactionResponseObject", JsonSerializer.Serialize(paymentModel));
					}
					if (PaymentInfo is SquareupChargesApiResponse)
					{
						SquareupChargesApiResponse paymentModel = (SquareupChargesApiResponse)PaymentInfo;
						objCmd.Parameters.AddWithValue("@TransactionId", paymentModel.payment.id);
						objCmd.Parameters.AddWithValue("@TransactionMethod", paymentModel.payment.card_details.card.card_type);
						objCmd.Parameters.AddWithValue("@ProcessResponseCode", paymentModel.payment.idempotency_key);
						objCmd.Parameters.AddWithValue("@ProcessResponseText", paymentModel.payment.status);
						objCmd.Parameters.AddWithValue("@TransactionResponseObject", JsonSerializer.Serialize(paymentModel));
					}
				}
				objCmd.Parameters.AddWithValue("@Notes", Notes);
				objCmd.Parameters.AddWithValue("@MaxDurationofSlab", model.MaxDurationofSlab);
				objCmd.Parameters.AddWithValue("@MaxRateofSlab", model.MaxRateofSlab);
				objCmd.Parameters.AddWithValue("@SendeTicket", (model.IsPaymentFromCustomerSite == true || model.PaymentMode.ToLower().Equals(EPaymentMode.Electronic.ToString().ToLower()) ? true : model.SendeTicket));
				objCmd.Parameters.AddWithValue("@BookingDetailRef", MapDataTable.ToDataTable(lstBookingDetails));


				DataTable dt = objSQL.FetchDT(objCmd);
				if (dt.Rows.Count > 0)
				{
					var Error = Convert.ToString(dt.Rows[0]["Error"]);
					if (!string.IsNullOrEmpty(Error))
						throw new AppException(Error);
					return new BookingIdResponse { BookingId = Convert.ToInt64(dt.Rows[0]["BookingId"]) };
				}
				return null;
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


		public GuestPrebookingResponse GetAvailableParkingDetails(GuestPreBookingRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetAvailableParkingSlot_v4");
			try
			{
				TimeSpan StartTime = TimeSpan.Parse(model.StartTime), EndTime = TimeSpan.Parse(model.EndTime), TimeDiff;
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());

				TimeDiff = (model.EndDate + EndTime) - (model.StartDate + StartTime);
				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);

				CurrentLocationRequest currentRequest = _mapper.Map<CurrentLocationRequest>(model);

				var (TotalHours1, lstSearch) = parkingHelper.GetSearchDateTimingwiseTable(currentRequest, timeZoneInfo);

				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@VehicleTypeId", model.VehicleTypeId);
				objCmd.Parameters.AddWithValue("@StartDate", model.StartDate);
				objCmd.Parameters.AddWithValue("@EndDate", model.EndDate);
				objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);
				objCmd.Parameters.AddWithValue("@NumberPlate", model.NumberPlate);
				objCmd.Parameters.AddWithValue("@SearchParkingSlots", MapDataTable.ToDataTable(lstSearch));

				DataSet ds = objSQL.FetchDB(objCmd);
				var error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);

				if (!string.IsNullOrEmpty(error))
					throw new AppException(error);

				var data = (from DataRow dr in ds.Tables[0].Rows
							select new
							{
								ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
								Symbol = Convert.ToString(dr["Symbol"]),
								TaxPercent = Convert.ToDecimal(dr["Tax"]),
								HasPaymentSetup = Convert.ToBoolean(dr["HasPaymentSetup"]),
								ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
								IsWhiteListCustomer = Convert.ToBoolean(dr["IsWhiteListCustomer"]),
								IsChargeBackCustomer = Convert.ToBoolean(dr["IsChargeBackCustomer"]),
								LocationId = Convert.ToString(dr["LocationId"]),
								AccessToken = Convert.ToString(dr["AccessToken"]),
								PaymentMethod = Convert.ToString(dr["PaymentMethod"])
							}).FirstOrDefault();


				var slotDetails = _mapper.Map<GuestPrebookingResponse>(model);

				var rates = (from DataRow dr in ds.Tables[1].Rows
							 select new ParkingLocationRateRequest
							 {
								 Duration = Convert.ToInt32(dr["DurationUpto"]),
								 Charges = Convert.ToDecimal(dr["Rate"])
							 }).ToList();


				// var (duration, charges) = parkingHelper.FetchMaxDurationandCharges(rates, TotalHours);
				// ParkingLocationRateRequest rate = new ParkingLocationRateRequest { Duration = Convert.ToInt32(duration), Charges = charges };

				// slotDetails.PerHourRate = Convert.ToDecimal(Convert.ToDouble(rate.Charges) / TotalHours);

				// slotDetails.OverSizedCharges = data.ExtraCharges;
				// slotDetails.TotalAmount = rate.Charges;
				// slotDetails.FinalAmount = slotDetails.TotalAmount + slotDetails.OverSizedCharges;
				// slotDetails.TaxAmount = parkingHelper.RoundOff(data.TaxPercent > 0.00m ? ((slotDetails.FinalAmount * data.TaxPercent) / 100) : 0.00m);
				// slotDetails.FinalAmount += slotDetails.TaxAmount;

				// slotDetails.MaxDurationofSlab = rate.Duration;
				// slotDetails.MaxRateofSlab = rate.Charges;


				var (TotalHours, PerHourRate, rate) = parkingHelper.GetTotalHoursandAmountByDuration(rates, DateTime.Parse(model.StartDate.ToShortDateString()), DateTime.Parse(model.EndDate.ToShortDateString()), model.StartTime, model.EndTime, model.IsFullTimeBooking);

				decimal TotalAmount = model.IsFullTimeBooking ? rate.Charges : parkingHelper.RoundOff(PerHourRate * TotalHours);

				slotDetails.PerHourRate = PerHourRate;
				slotDetails.OverSizedCharges = data.ExtraCharges;
				slotDetails.TotalAmount = TotalAmount;
				slotDetails.FinalAmount = TotalAmount + slotDetails.OverSizedCharges;
				slotDetails.MaxDurationofSlab = rate.Duration;
				slotDetails.MaxRateofSlab = rate.Charges;
				slotDetails.ConvenienceFee = data.ConvenienceFee > 0.00m ? data.ConvenienceFee : 0.00m;
				slotDetails.TaxAmount = parkingHelper.RoundOff((data.TaxPercent > 0.00m ? ((slotDetails.FinalAmount * data.TaxPercent) / 100) : 0.00m));

				var TaxWithConvenienceFee = parkingHelper.RoundOff((data.TaxPercent > 0.00m ? (((slotDetails.FinalAmount + slotDetails.ConvenienceFee) * data.TaxPercent) / 100) : 0.00m));
				slotDetails.TaxAmountWithConvenienceFee = TaxWithConvenienceFee;
				slotDetails.FinalAmountWithConvenienceFee = slotDetails.FinalAmount + slotDetails.TaxAmountWithConvenienceFee + slotDetails.ConvenienceFee;
				slotDetails.FinalAmount += slotDetails.TaxAmount;

				if (model.StartDate == model.EndDate)
				{
					var earlyBirdOffer = (from DataRow dr in ds.Tables[2].Rows
										  select new ParkingLocationEarlyBirdOffer
										  {
											  Id = Convert.ToInt64(dr["Id"]),
											  IsMonday = Convert.ToBoolean(dr["IsMonday"]),
											  IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
											  IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
											  IsThursday = Convert.ToBoolean(dr["IsThursday"]),
											  IsFriday = Convert.ToBoolean(dr["IsFriday"]),
											  IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
											  IsSunday = Convert.ToBoolean(dr["IsSunday"]),
											  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											  Amount = Convert.ToDecimal(dr["Amount"]),
											  EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
											  EnterToTime = Convert.ToString(dr["EnterToTime"]),
											  ExitByTime = Convert.ToString(dr["ExitByTime"]),
											  IsActive = Convert.ToBoolean(dr["IsActive"])
										  }).FirstOrDefault();

					var earlyBirdInfo = parkingHelper.GetEarlyBirdInfo(earlyBirdOffer, model.StartDate, model.StartTime
					, model.EndTime);


					slotDetails.IsEarlyBirdOfferApplicable = earlyBirdInfo == null ? false : true;
					slotDetails.EarlyBirdInfo = earlyBirdInfo;

					if (earlyBirdInfo != null)
					{
						slotDetails.EarlyBirdFinalAmount = slotDetails.IsEarlyBirdOfferApplicable ? earlyBirdInfo.Amount + slotDetails.OverSizedCharges : (decimal?)null;

						slotDetails.EarlyBirdTaxAmount = parkingHelper.RoundOff(data.TaxPercent > 0.00m ? ((slotDetails.EarlyBirdFinalAmount * data.TaxPercent) / 100) : 0.00m);

						slotDetails.EarlyBirdFinalAmount += slotDetails.EarlyBirdTaxAmount;

						var EarlyBirdTax = parkingHelper.RoundOff(data.TaxPercent > 0.00m ? (((earlyBirdInfo.Amount + slotDetails.ConvenienceFee) * data.TaxPercent) / 100) : 0.00m);

						slotDetails.EarlyBirdTaxAmountWithConvenienceFee = EarlyBirdTax; ;
						slotDetails.EarlyBirdFinalAmountWithConvenienceFee = earlyBirdInfo.Amount + slotDetails.EarlyBirdTaxAmountWithConvenienceFee + slotDetails.ConvenienceFee;
					}
                }

                var nightFareOffer = (from DataRow dr in ds.Tables[3].Rows
                                      select new ParkingLocationNightFareOffer
                                      {
                                          Id = Convert.ToInt64(dr["Id"]),
                                          IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                          IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                          IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                          IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                          IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                          IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                          IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                          ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                          Amount = Convert.ToDecimal(dr["Amount"]),
                                          EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
                                          EnterToTime = Convert.ToString(dr["EnterToTime"]),
                                          ExitByTime = Convert.ToString(dr["ExitByTime"]),
                                          IsActive = Convert.ToBoolean(dr["IsActive"])
                                      }).FirstOrDefault();

                var nightFareInfo = parkingHelper.GetNightFareInfo(nightFareOffer, model.StartDate, model.EndDate, model.StartTime
                , model.EndTime);


                slotDetails.IsNightFareOfferApplicable = nightFareInfo == null ? false : true;
                slotDetails.NightFareInfo = nightFareInfo;

                if (nightFareInfo != null)
                {
                    slotDetails.NightFareFinalAmount = slotDetails.IsNightFareOfferApplicable ? nightFareInfo.Amount + slotDetails.OverSizedCharges : (decimal?)null;

                    slotDetails.NightFareTaxAmount = parkingHelper.RoundOff(data.TaxPercent > 0.00m ? ((slotDetails.NightFareFinalAmount * data.TaxPercent) / 100) : 0.00m);

                    slotDetails.NightFareFinalAmount += slotDetails.NightFareTaxAmount;

                    var NightFareTax = parkingHelper.RoundOff(data.TaxPercent > 0.00m ? (((nightFareInfo.Amount + slotDetails.ConvenienceFee) * data.TaxPercent) / 100) : 0.00m);

                    slotDetails.NightFareTaxAmountWithConvenienceFee = NightFareTax; ;
                    slotDetails.NightFareFinalAmountWithConvenienceFee = nightFareInfo.Amount + slotDetails.NightFareTaxAmountWithConvenienceFee + slotDetails.ConvenienceFee;
                }

                // var MaxStay = ((int)TimeDiff.TotalHours > 0 ? (int)TimeDiff.TotalHours + " hrs" : "") + (TimeDiff.Minutes > 0 ? " " + TimeDiff.Minutes + " mins" : "");
                // slotDetails.MaxStay = MaxStay.Trim();
                var minutes = (int)((TotalHours - Math.Truncate(TotalHours)) * 60);
				var duration = (int)TotalHours + " hours" + (minutes == 0 ? "" : " " + minutes + " minutes");
				slotDetails.MaxStay = duration;

				slotDetails.Symbol = data.Symbol;
				slotDetails.IsChargeBackCustomer = data.IsChargeBackCustomer;
				slotDetails.IsWhiteListCustomer = data.IsWhiteListCustomer;
				slotDetails.HasPaymentSetup = data.HasPaymentSetup;
				slotDetails.LocationId = data.PaymentMethod == EPaymentMode.Square.ToString() ? data.LocationId : "";
				slotDetails.AccessToken = data.PaymentMethod == EPaymentMode.Square.ToString() ? data.AccessToken : "";
				return slotDetails;
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

		public long UpdateNotesforParkingSlot(UpdateNotesRequest model)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateNotesForParkingSlots");
			try
			{
				objCmd.Parameters.AddWithValue("@EnterExitId", model.EnterExitId);
				objCmd.Parameters.AddWithValue("@Notes", model.Notes);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				return Convert.ToInt64(dtCustomer.Rows[0]["Id"]);
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

		public ParkingLocDetailsResponse GetEstimatedBookingDetailsByLoc(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, string BookingType, bool IsFullTimeBooking, long? CustomerVehicleId, string TimeZone, bool IsFromQRScan)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetEstimatedBookingDetailsByLoc");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				objCmd.Parameters.AddWithValue("@BookingType", BookingType);
				if (IsFromQRScan)
				{

					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);

					CurrentLocationRequest currentRequest = new CurrentLocationRequest
					{
						BookingType = BookingType,
						IsFullTimeBooking = IsFullTimeBooking,
						StartDate = StartDate,
						StartTime = StartTime,
						EndDate = EndDate,
						EndTime = EndTime
					};

					var (vTotalHours, lstSearch) = parkingHelper.GetSearchDateTimingwiseTable(currentRequest, timeZoneInfo);

					objCmd.Parameters.AddWithValue("@StartDate", StartDate);
					objCmd.Parameters.AddWithValue("@EndDate", EndDate);
					objCmd.Parameters.AddWithValue("@IsFromQRScan", IsFromQRScan);
					objCmd.Parameters.AddWithValue("@SearchParkingSlots", MapDataTable.ToDataTable(lstSearch));
				}

				DataSet ds = objSQL.FetchDB(objCmd);

				var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);

				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);


				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				var plocation = (from DataRow dr in ds.Tables[1].Rows
								 select new ParkingLocDetailsResponse
								 {
									 Id = Convert.ToInt64(dr["Id"]),
									 LocationName = Convert.ToString(dr["LocationName"]),
									 Address = dr["Address"].ToString(),
									 City = dr["City"].ToString(),
									 State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
									 StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
									 Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
									 CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
									 ZipCode = dr["ZipCode"].ToString(),
									 LocationPic = dr["LocationPic"].ToString(),
									 Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
									 MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
									 OverSizedChargesMonthly = Convert.ToDecimal(dr["OverSizedChargesMonthly"]),
									 OverSizedChargesRegular = Convert.ToDecimal(dr["OverSizedChargesRegular"]),
									 PaymentMethod = dr["PaymentGateway"] == DBNull.Value ? null : Convert.ToString(dr["PaymentGateway"]),
									 ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
									 SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
									 AccessToken = dr["AccessToken"] == DBNull.Value ? null : Convert.ToString(dr["AccessToken"]),
									 IsProduction = dr["IsProduction"] == DBNull.Value ? false : Convert.ToBoolean(dr["IsProduction"]),
									 ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
									 LocationId = dr["LocationId"] == DBNull.Value ? null : Convert.ToString(dr["LocationId"]),
									 TaxPercent = Convert.ToDecimal(dr["Tax"]),
									 Currency = Convert.ToString(dr["Currency"]),
									 Symbol = Convert.ToString(dr["Symbol"]),
									 BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
									 LogoUrl = Convert.ToString(dr["LogoUrl"]),
									 ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
								 }).FirstOrDefault();

				if (plocation == null)
					throw new AppException("Location not found");

				var timings = (from DataRow dr in ds.Tables[2].Rows
							   select new ParkingLocationTiming
							   {
								   IsMonday = Convert.ToBoolean(dr["IsMonday"]),
								   IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
								   IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
								   IsThursday = Convert.ToBoolean(dr["IsThursday"]),
								   IsFriday = Convert.ToBoolean(dr["IsFriday"]),
								   IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
								   IsSunday = Convert.ToBoolean(dr["IsSunday"]),
								   StartDate = Convert.ToDateTime(dr["StartDate"]),
								   StartDateUtc = Convert.ToDateTime(dr["StartDateUtc"]),
								   StartTime = TimeSpan.Parse(dr["StartTime"].ToString()),
								   EndTime = TimeSpan.Parse(dr["EndTime"].ToString())
							   }).ToList();

				plocation.ParkingTimings = parkingHelper.GetTimings(timings);

				var rates = (from DataRow dr in ds.Tables[3].Rows
							 select new ParkingLocationRateRequest
							 {
								 Duration = BookingType.ToLower() == "monthly" ?Convert.ToInt32(dr["DurationUpto"]) * 30 : Convert.ToInt32(dr["DurationUpto"]),
								 Charges = Convert.ToDecimal(dr["Rate"])
							 }).ToList();

				if (rates == null || rates.Count == 0)
					throw new AppException("Rates aren't specified for the provided location.");

				var (TotalHours, PerHourRate, rate) = parkingHelper.GetTotalHoursandAmountByDuration(rates, DateTime.Parse(StartDate.ToShortDateString()), DateTime.Parse(EndDate.ToShortDateString()), StartTime, EndTime, IsFullTimeBooking);


				decimal TotalAmount = IsFullTimeBooking ? rate.Charges : parkingHelper.RoundOff(PerHourRate * TotalHours);


				plocation.PerHourRate = PerHourRate;
				plocation.TotalHours = BookingType.ToLower() == "monthly" ? Math.Round(TotalHours) : TotalHours;
				plocation.TotalAmount = TotalAmount;
				plocation.FinalAmount = TotalAmount;
				plocation.OverSizedCharges = plocation.OverSizedCharges ?? 0.00m;
				plocation.MaxDurationofSlab = rate.Duration;
				plocation.MaxRateofSlab = rate.Charges;
				var CustInfo = (from DataRow dr in ds.Tables[4].Rows
								select new
								{
									Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
									CustomerId = Convert.ToInt64(dr["CustomerId"]),
									NumberPlate = Convert.ToString(dr["NumberPlate"]),
									VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
									Address = Convert.ToString(dr["Address"]),
									VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["VehicleTypeId"])
								}).ToList();




				if (CustInfo != null && CustInfo.Count > 0)
				{
					if (CustomerVehicleId != null && CustInfo.Find(a => a.Id.Equals(CustomerVehicleId) && a.VehicleTypeId.Equals(2)) != null)
					{
						plocation.OverSizedCharges = BookingType.ToLower().Equals("hourly") ? plocation.OverSizedChargesRegular : plocation.OverSizedChargesMonthly;
						plocation.IsOverSizedVehicle = true;
						plocation.FinalAmount += Convert.ToDecimal(plocation.OverSizedCharges);

					}
					plocation.CustomerDetails = new CustomerDetails
					{
						Address = CustInfo.Select(a => a.Address).FirstOrDefault(),
						CustomerId = CustInfo.Select(a => a.CustomerId).FirstOrDefault(),
						CustomerVehicles = CustInfo.FirstOrDefault().Id != null ?
					CustInfo.AsEnumerable().Select(a => new VehicleDetails
					{
						Id = a.Id,
						NumberPlate = a.NumberPlate,
						VehicleModal = a.VehicleModal
					}).Where(e => e.NumberPlate.ToLower() != "unknown").ToList() : null
					};
				}
				plocation.FinalAmount += plocation.ConvenienceFee;
				plocation.TaxAmount = parkingHelper.RoundOff((plocation.TaxPercent > 0.00m ? ((plocation.FinalAmount * plocation.TaxPercent) / 100) : 0.00m));
				plocation.FinalAmount += plocation.TaxAmount;


				// early bird check
				if (StartDate == EndDate)
				{
					var earlyBirdOffer = (from DataRow dr in ds.Tables[5].Rows
										  select new ParkingLocationEarlyBirdOffer
										  {
											  Id = Convert.ToInt64(dr["Id"]),
											  IsMonday = Convert.ToBoolean(dr["IsMonday"]),
											  IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
											  IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
											  IsThursday = Convert.ToBoolean(dr["IsThursday"]),
											  IsFriday = Convert.ToBoolean(dr["IsFriday"]),
											  IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
											  IsSunday = Convert.ToBoolean(dr["IsSunday"]),
											  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											  Amount = Convert.ToDecimal(dr["Amount"]),
											  EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
											  EnterToTime = Convert.ToString(dr["EnterToTime"]),
											  ExitByTime = Convert.ToString(dr["ExitByTime"]),
											  IsActive = Convert.ToBoolean(dr["IsActive"])
										  }).FirstOrDefault();

					var earlyBirdInfo = parkingHelper.GetEarlyBirdInfo(earlyBirdOffer, StartDate, StartTime
					, EndTime);
					plocation.IsEarlyBirdOfferApplicable = earlyBirdInfo == null ? false : true;

					plocation.EarlyBirdFinalAmount = plocation.IsEarlyBirdOfferApplicable ? earlyBirdInfo.Amount + plocation.OverSizedCharges + plocation.ConvenienceFee : (decimal?)null;
					plocation.EarlyBirdTaxAmount = parkingHelper.RoundOff(plocation.TaxPercent > 0.00m ? ((plocation.EarlyBirdFinalAmount * plocation.TaxPercent) / 100) : 0.00m);
					plocation.EarlyBirdFinalAmount += plocation.EarlyBirdTaxAmount;
					plocation.EarlyBirdInfo = earlyBirdInfo;
                }

                var nightFareOffer = (from DataRow dr in ds.Tables[6].Rows
                                      select new ParkingLocationNightFareOffer
                                      {
                                          Id = Convert.ToInt64(dr["Id"]),
                                          IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                          IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                          IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                          IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                          IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                          IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                          IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                          ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                          Amount = Convert.ToDecimal(dr["Amount"]),
                                          EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
                                          EnterToTime = Convert.ToString(dr["EnterToTime"]),
                                          ExitByTime = Convert.ToString(dr["ExitByTime"]),
                                          IsActive = Convert.ToBoolean(dr["IsActive"])
                                      }).FirstOrDefault();

                var nightFareInfo = parkingHelper.GetNightFareInfo(nightFareOffer, StartDate, EndDate, StartTime, EndTime);

                plocation.IsNightFareOfferApplicable = nightFareInfo == null ? false : true;

                plocation.NightFareFinalAmount = plocation.IsNightFareOfferApplicable ? nightFareInfo.Amount + plocation.OverSizedCharges + plocation.ConvenienceFee : (decimal?)null;
                plocation.NightFareTaxAmount = parkingHelper.RoundOff(plocation.TaxPercent > 0.00m ? ((plocation.NightFareFinalAmount * plocation.TaxPercent) / 100) : 0.00m);
                plocation.NightFareFinalAmount += plocation.NightFareTaxAmount;
                plocation.NightFareInfo = nightFareInfo;

                plocation.StartDate = StartDate;
				plocation.EndDate = EndDate;
				plocation.StartTime = StartTime;
				plocation.EndTime = EndTime;


				var minutes = (int)((plocation.TotalHours - Math.Truncate(plocation.TotalHours)) * 60);
				var duration = (int)plocation.TotalHours + " hours" + (minutes == 0 ? "" : " " + minutes + " minutes");
				plocation.MaxStay = duration;
				return plocation;
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

		public BookingsByOwnerResponse GetBookingByParkingOwner(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BookingType)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingsByParkingOwner_v2");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@BookingType", BookingType);

				DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new BookingList
										{
											BookingId = Convert.ToInt64(dr["BookingId"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											CustomerName = Convert.ToString(dr["CustomerName"]),
											TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
											StartDate = Convert.ToDateTime(dr["StartDate"]),
											EndDate = Convert.ToDateTime(dr["EndDate"]),
											StartTime = Convert.ToString(dr["StartTime"]),
											EndTime = Convert.ToString(dr["EndTime"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											BookingType = Convert.ToString(dr["BookingType"]),
											UnpaidAmount = Convert.ToDecimal(dr["UnpaidAmount"]),
											IsCancelled = Convert.ToBoolean(dr["IsCancelled"])
										}).ToList();

					return new BookingsByOwnerResponse { Bookings = lstlocations, Total = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]) };
				}
				return null;
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

		public List<ParkingLocationName> GetLocationNamesFromList(string Locations)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetLocationNamesFromList");
			try
			{
				objCmd.Parameters.AddWithValue("@Locations", Locations);
				DataTable dtLocations = objSQL.FetchDT(objCmd);


				var lstlocations = (from DataRow dr in dtLocations.Rows
									select new ParkingLocationName
									{
										Id = Convert.ToInt64(dr["Id"]),
										LocationName = Convert.ToString(dr["LocationName"])
									}).ToList();


				return lstlocations;
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


		public List<DamageVehicleReport> GetDamageVehicleReport(long ParkingLocationId, string CurrentDate, string SearchValue)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetDamageReport");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				DataSet dsDamageReport = objSQL.FetchDB(objCmd);

				var damageImages = (from DataRow dr in dsDamageReport.Tables[1].Rows
									select new DamageVehicleImages
									{
										DamageVehicleId = Convert.ToInt64(dr["DamageVehicleId"]),
										ImageURL = Convert.ToString(dr["Filepath"])
									}).ToList();

				var damageVehicle = (from DataRow dr in dsDamageReport.Tables[0].Rows
									 select new DamageVehicleReport
									 {
										 DamageVehicleId = Convert.ToInt64(dr["DamageVehicleId"]),
										 ValetName = Convert.ToString(dr["ValetName"]),
										 CustomerName = Convert.ToString(dr["CustomerName"]),
										 NumberPlate = Convert.ToString(dr["NumberPlate"]),
										 ReportedOn = Convert.ToDateTime(dr["ReportedOn"]),
										 Mobile = Convert.ToString(dr["Mobile"]),
										 Notes = Convert.ToString(dr["Notes"]),
										 VehicleModal = Convert.ToString(dr["VehicleModal"]),
										 Images = (damageImages == null || damageImages.Count == 0) ? null : damageImages.Where(a => a.DamageVehicleId.Equals(Convert.ToInt64(dr["DamageVehicleId"]))).Select(a => a).ToList()
									 }).ToList();


				return damageVehicle;
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

		public CheckInReportResponse GetCheckInReport(long ParkingLocationId, string CurrentDate, string SearchValue, bool HasCheckedIn)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCheckInReport");
			try
			{
				DateTime currentDate = DateTime.Parse(CurrentDate);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CurrentDate", currentDate.Date);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@HasCheckedIn", HasCheckedIn);


				DataTable dtCheckin = objSQL.FetchDT(objCmd);

				var Active = (from DataRow dr in dtCheckin.Rows
							  where Convert.ToBoolean(dr["IsActive"]) == true
							  select new ActiveUserResponse
							  {
								  Id = Convert.ToInt64(dr["Id"]),
								  StaffName = Convert.ToString(dr["StaffName"]),
								  Mobile = Convert.ToString(dr["Mobile"]),
								  CheckInTime = dr["CheckInTime"] == DBNull.Value ? "NIL" : dateTimeHelper.GetDateFormatBasedonCurrentDate(Convert.ToDateTime(dr["CheckInTime"]), currentDate.Date),
								  LastCheckOut = dr["LastCheckOut"] == DBNull.Value ? "NIL" : dateTimeHelper.GetDateFormatBasedonCurrentDate(Convert.ToDateTime(dr["LastCheckOut"]), currentDate.Date),
								  IsActive = Convert.ToBoolean(dr["IsActive"])
							  }).ToList();

				var InActive = (from DataRow dr in dtCheckin.Rows
								where Convert.ToBoolean(dr["IsActive"]) == false
								select new InActiveUserResponse
								{
									Id = Convert.ToInt64(dr["Id"]),
									StaffName = Convert.ToString(dr["StaffName"]),
									Mobile = Convert.ToString(dr["Mobile"]),
									CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? "NIL" : dateTimeHelper.GetDateFormatBasedonCurrentDate(Convert.ToDateTime(dr["CheckOutTime"]), currentDate.Date),
									LastCheckInTime = dr["CheckInTime"] == DBNull.Value ? "NIL" : dateTimeHelper.GetDateFormatBasedonCurrentDate(Convert.ToDateTime(dr["CheckInTime"]), currentDate.Date),
									IsActive = Convert.ToBoolean(dr["IsActive"])
								}).ToList();

				return new CheckInReportResponse
				{
					ActiveUsers = Active,
					InActiveUsers = InActive
				};
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

		public DepositReportResponse GetDepositReport(string ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetDepositReport");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				DataTable dtReport = objSQL.FetchDT(objCmd);

				if (dtReport.Rows.Count > 0)
				{
					var depositReports = (from DataRow dr in dtReport.Rows
										  select new DepositReport
										  {
											  UserName = Convert.ToString(dr["Name"]),
											  RoleName = (dr["RoleName"].ToString()) == "ParkingManager" ? "Manager" : Convert.ToString(dr["RoleName"]),
											  DepositedDate = Convert.ToDateTime(dr["DepositedDate"]),
											  DepositedVia = Convert.ToString(dr["DepositedVia"]),
											  Amount = Convert.ToDecimal(dr["Amount"]),
											  LocationName = Convert.ToString(dr["LocationName"])
										  }).ToList();

					if (depositReports == null || depositReports.Count == 0)
						return new DepositReportResponse { DepositReport = new List<DepositReport>(), TotalAmount = 0.00m };

					return new DepositReportResponse { DepositReport = depositReports, Total = Convert.ToInt32(dtReport.Rows[0]["TotalCount"]), TotalAmount = depositReports.Sum(a => a.Amount) };
				}
				return new DepositReportResponse { DepositReport = new List<DepositReport>(), TotalAmount = 0.00m };

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

		public long AddLocationCameraSettings(LocationCameraSettings model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddLocationCameraSettings");
			try
			{
				objCmd.Parameters.AddWithValue("@LocationCameraId", model.LocationCameraId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CameraId", model.CameraId);
				objCmd.Parameters.AddWithValue("@IsForEntry", model.IsForEntry);
				DataTable dtLocation = objSQL.FetchDT(objCmd);

				return Convert.ToInt64(dtLocation.Rows[0]["LocationCameraId"]);
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

		public LocationCameraSettings GetLocationCameraSettings(long LocationCameraId)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetLocationCameraSettings");
			try
			{
				objCmd.Parameters.AddWithValue("@LocationCameraId", LocationCameraId);

				DataTable dtSettings = objSQL.FetchDT(objCmd);

				var cameraSettings = (from DataRow dr in dtSettings.Rows
									  select new LocationCameraSettings
									  {
										  LocationCameraId = Convert.ToInt64(dr["Id"]),
										  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										  CameraId = Convert.ToString(dr["CameraId"]),
										  IsForEntry = Convert.ToBoolean(dr["IsForEntry"]),
										  CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
										  UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null

									  }).FirstOrDefault();

				return cameraSettings;
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

		public List<CameraIdListResponse> GetCameraListByLocation(long ParkingLocationId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCameraListByLocation");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);

				DataSet dsLocations = objSQL.FetchDB(objCmd);

				var lstCamera = (from DataRow dr in dsLocations.Tables[0].Rows
								 select new CameraIdListResponse
								 {
									 CameraId = Convert.ToString(dr["CameraId"])
								 }).ToList();
				return lstCamera;
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

		public CameraSettingListResponse GetCameraSettingList(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCameraSettingDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);


				DataTable dtCamera = objSQL.FetchDT(objCmd);
				if (dtCamera.Rows.Count > 0)
				{
					var listCameraSetting = (from DataRow dr in dtCamera.Rows
											 select new CameraSettingList
											 {
												 Id = Convert.ToInt64(dr["Id"]),
												 LocationName = Convert.ToString(dr["LocationName"]),
												 CameraId = Convert.ToString(dr["CameraId"]),
												 IsForEntry = Convert.ToBoolean(dr["IsForEntry"]),

											 }).ToList();

					return new CameraSettingListResponse
					{
						CameraSettingList = listCameraSetting,
						Total = Convert.ToInt32(dtCamera.Rows[0]["TotalCount"])
					};
				}
				return null;
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


		public long DeleteCameraSettings(LocationCameraIdModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_DeleteCameraSettings");
			try
			{
				objCmd.Parameters.AddWithValue("@LocationCameraId", model.LocationCameraId);
				DataTable dtLoc = objSQL.FetchDT(objCmd);
				var Error = Convert.ToString(dtLoc.Rows[0]["Error"]);

				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.LocationCameraId;
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

		public string GetLocationQRCode(long ParkingLocationId, string LogoUrl)
		{
			string QRCode = string.Empty;
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateLocationQR");
			try
			{
				string url = $"https://flixvalet.com/qr?redirect_url=home/BookParking/{ParkingLocationId}"; // to be replaced later.
				QRCode = _qRRepo.GetDynamicTigerQRImage(url, LogoUrl ?? _appsettings.LogoUrl);

				if (string.IsNullOrEmpty(QRCode))
					throw new AppException("An error occurred while creating the QR code image.");

				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@QRCodePath", QRCode);
				objSQL.UpdateDB(objCmd, true);

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

			return QRCode;

		}


		public PostBookingModel BookParkingLocation_v1(BookingRequest model, object PaymentInfo = null)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_BookParkingLocation_v11");
			try
			{
				TimeSpan StartTime = TimeSpan.Parse(model.StartTime),
				EndTime = TimeSpan.Parse(model.EndTime), perDayDurationSpan = new TimeSpan();
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());

				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);

				//calculating per day hour difference not considered in case of full time booking
				if (!model.IsFullTimeBooking)
				{
					perDayDurationSpan = parkingHelper.GetMonthlyBookingTimeDifference(model.StartDate, StartTime, EndTime);
				}

				var (TotalHours, lstBookingDetails) = parkingHelper.GetBookingDetails(model, timeZoneInfo);


				if (model.IsEarlyBirdOfferApplied)
				{

					var EBhours = parkingHelper.CalculateDuration(model.StartDate.Date, TimeSpan.Parse(model.StartTime), TimeSpan.Parse(model.EndTime));
					model.MaxDurationofSlab = Convert.ToDouble(EBhours);
					model.MaxRateofSlab = model.EarlyBirdAmount;
				}

                if (model.IsNightFareOfferApplied)
                {
                    var NFhours = parkingHelper.CalculateDuration(model.StartDate.Date, TimeSpan.Parse(model.StartTime), TimeSpan.Parse(model.EndTime));
                    model.MaxDurationofSlab = Convert.ToDouble(NFhours);
                    model.MaxRateofSlab = model.NightFareAmount;
                }

                var ConvenienceFee = model.PaymentMode?.ToLower() == EPaymentMode.PayLater.ToString().ToLower() ? 0 : model.ConvenienceFee;

				model.OverSizedCharges = model.OverSizedCharges ?? 0.00m;
				objCmd.Parameters.AddWithValue("@Id", model.Id);
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);
				objCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@IsEarlyBirdOfferApplied", model.IsEarlyBirdOfferApplied);
                objCmd.Parameters.AddWithValue("@IsNightFareOfferApplied", model.IsNightFareOfferApplied);
                objCmd.Parameters.AddWithValue("@EarlyBirdId", model.EarlyBirdId);
                objCmd.Parameters.AddWithValue("@NightFareId", model.NightFareId);

                objCmd.Parameters.AddWithValue("@BookingAmount", model.IsEarlyBirdOfferApplied ? model.EarlyBirdAmount : model.IsNightFareOfferApplied ? model.NightFareAmount :  model.TotalAmount);
                objCmd.Parameters.AddWithValue("@OverSizedCharges", model.OverSizedCharges);
				objCmd.Parameters.AddWithValue("@TaxAmount", model.IsEarlyBirdOfferApplied ? model.EarlyBirdTaxAmount : model.IsNightFareOfferApplied ? model.NightFareTaxAmount : model.TaxAmount);
				objCmd.Parameters.AddWithValue("@FinalAmount", model.IsEarlyBirdOfferApplied ? model.EarlyBirdFinalAmount : model.IsNightFareOfferApplied ? model.NightFareFinalAmount : model.FinalAmount);
				objCmd.Parameters.AddWithValue("@IsPaymentFromCustomerSite", model.IsPaymentFromCustomerSite);
				objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);
				objCmd.Parameters.AddWithValue("@ConvenienceFee", ConvenienceFee);
				objCmd.Parameters.AddWithValue("@BookingCategoryId", model.BookingCategoryId);
				objCmd.Parameters.AddWithValue("@BookingNotes", model.Notes);

				if (model.PaymentMode?.ToLower() == EPaymentMode.SquareCard.ToString().ToLower())
				{
					objCmd.Parameters.AddWithValue("@TransactionId", model.TransactionId);
				}
				var Notes = (model.StartDate + StartTime).ToString("MMM dd h:mm tt") + " to " + (model.EndDate + EndTime).ToString("MMM dd h:mm tt");


				if (PaymentInfo != null)
				{
					if (PaymentInfo is StripeChargesApiResponse)
					{
						StripeChargesApiResponse paymentModel = (StripeChargesApiResponse)PaymentInfo;
						objCmd.Parameters.AddWithValue("@TransactionId", paymentModel.id);
						objCmd.Parameters.AddWithValue("@TransactionMethod", paymentModel.card.funding);
						objCmd.Parameters.AddWithValue("@ProcessResponseCode", paymentModel.balance_transaction);
						objCmd.Parameters.AddWithValue("@ProcessResponseText", paymentModel.outcome == null ? null : paymentModel.outcome.seller_message);
						objCmd.Parameters.AddWithValue("@TransactionResponseObject", JsonSerializer.Serialize(paymentModel));
					}
					if (PaymentInfo is SquareupChargesApiResponse)
					{
						SquareupChargesApiResponse paymentModel = (SquareupChargesApiResponse)PaymentInfo;
						objCmd.Parameters.AddWithValue("@TransactionId", paymentModel.payment.id);
						objCmd.Parameters.AddWithValue("@TransactionMethod", paymentModel.payment.card_details.card.card_type);
						objCmd.Parameters.AddWithValue("@ProcessResponseCode", paymentModel.payment.idempotency_key);
						objCmd.Parameters.AddWithValue("@ProcessResponseText", paymentModel.payment.status);
						objCmd.Parameters.AddWithValue("@TransactionResponseObject", JsonSerializer.Serialize(paymentModel));
					}
					if (PaymentInfo is PaypalPaymentApiResponse)
					{
						PaypalPaymentApiResponse paymentModel = (PaypalPaymentApiResponse)PaymentInfo;
						objCmd.Parameters.AddWithValue("@TransactionId", paymentModel.Target.Id);
						objCmd.Parameters.AddWithValue("@TransactionMethod", paymentModel.Target.CreditCard.AccountType);
						objCmd.Parameters.AddWithValue("@ProcessResponseCode", paymentModel.Target.ProcessorResponseCode);
						objCmd.Parameters.AddWithValue("@ProcessResponseText", paymentModel.Target.ProcessorResponseText);
						objCmd.Parameters.AddWithValue("@TransactionResponseObject", JsonSerializer.Serialize(paymentModel));
					}
				}
				objCmd.Parameters.AddWithValue("@Notes", Notes);
				objCmd.Parameters.AddWithValue("@MaxDurationofSlab", model.MaxDurationofSlab);
				objCmd.Parameters.AddWithValue("@MaxRateofSlab", model.MaxRateofSlab);
				objCmd.Parameters.AddWithValue("@SendeTicket", (model.IsPaymentFromCustomerSite == true || model.PaymentMode.ToLower().Equals(EPaymentMode.Electronic.ToString().ToLower()) ? true : model.SendeTicket));

				if (model.GuestInfo != null)
				{
					objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                    objCmd.Parameters.AddWithValue("@Email", model.Email);
                    objCmd.Parameters.AddWithValue("@NumberPlate", model.GuestInfo.NumberPlate);
					objCmd.Parameters.AddWithValue("@VehicleModal", model.GuestInfo.VehicleModal);
					objCmd.Parameters.AddWithValue("@VehicleTypeId", model.GuestInfo.VehicleTypeId);
					objCmd.Parameters.AddWithValue("@VehicleColorId", model.GuestInfo.VehicleColorId);
					objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.GuestInfo.VehicleManufacturerId);
					objCmd.Parameters.AddWithValue("@StateCode", model.GuestInfo.StateCode);
					objCmd.Parameters.AddWithValue("@CountryCode", model.GuestInfo.CountryCode);
				}
				objCmd.Parameters.AddWithValue("@IsGuestAddRequired", model.IsGuestAddRequired);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
				objCmd.Parameters.AddWithValue("@IsFromQRScan", model.IsFromQRScan);
				objCmd.Parameters.AddWithValue("@BookingDetailRef", MapDataTable.ToDataTable(lstBookingDetails));


				DataSet ds = objSQL.FetchDB(objCmd);

				PostBookingModel postBookingModel = null;

				if (ds.Tables[0].Rows.Count > 0)
				{

					postBookingModel = new PostBookingModel();
					var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
					if (!string.IsNullOrEmpty(Error))
						throw new AppException(Error);


					postBookingModel = (from DataRow dr in ds.Tables[0].Rows
										select new PostBookingModel
										{
											BookingId = Convert.ToInt64(dr["BookingId"]),
											CustomerId = Convert.ToInt64(dr["CustomerId"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											Symbol = Convert.ToString(dr["Symbol"]),
											LocationName = Convert.ToString(dr["LocationName"])
										}).FirstOrDefault();


					postBookingModel.BadgeCounts = (from DataRow dr in ds.Tables[0].Rows
													select new BadgeCount
													{
														CustomerBadgeCount = Convert.ToInt64(dr["CustomerBadgeCount"]),
														ValetBadgeCount = Convert.ToInt64(dr["ValetBadgeCount"])
													}).FirstOrDefault();

					DeviceToken devicetokens = new DeviceToken();

					devicetokens.StaffTokens = (from DataRow dr in ds.Tables[1].Rows
												select Convert.ToString(dr["DeviceToken"])
													).ToList();
					// postBookingModel.DeviceTokens.CustomerTokens
					devicetokens.CustomerTokens = (from DataRow dr in ds.Tables[2].Rows
												   select new CustomerToken
												   {
													   DeviceToken = Convert.ToString(dr["DeviceToken"]),
													   BrowserDeviceToken = Convert.ToString(dr["BrowserDeviceToken"])
												   }).FirstOrDefault();
					postBookingModel.DeviceTokens = devicetokens;

					postBookingModel.ListOwnerSupervisors = (from DataRow dr in ds.Tables[3].Rows
															 select Convert.ToString(dr["Email"])
												  ).ToList();
				}
				return postBookingModel;
			}
			catch (Exception ex)
			{
				throw;
			}
			finally
			{
				if (objSQL != null) objSQL.Dispose();
				if (objCmd != null) objCmd.Dispose();
			}
		}

        public PostBookingModel BookParkingLocationFromQR(BookingFromQrRequest model, string subscriptionId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_BookParkingLocationFromQR");
            try
            {
                TimeSpan StartTime = TimeSpan.Parse(model.StartTime),
                EndTime = TimeSpan.Parse(model.EndTime), perDayDurationSpan = new TimeSpan();
                model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
                model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);

                //calculating per day hour difference not considered in case of full time booking
                if (!model.IsFullTimeBooking)
                {
                    perDayDurationSpan = parkingHelper.GetMonthlyBookingTimeDifference(model.StartDate, StartTime, EndTime);
                }

                var (TotalHours, lstBookingDetails) = parkingHelper.GetBookingDetailsv1(model, timeZoneInfo);


                if (model.IsEarlyBirdOfferApplied)
                {

                    var EBhours = parkingHelper.CalculateDuration(model.StartDate.Date, TimeSpan.Parse(model.StartTime), TimeSpan.Parse(model.EndTime));
                    model.MaxDurationofSlab = Convert.ToDouble(EBhours);
                    model.MaxRateofSlab = model.EarlyBirdAmount;
                }

                if (model.IsNightFareOfferApplied)
                {
                    var NFhours = parkingHelper.CalculateDuration(model.StartDate.Date, TimeSpan.Parse(model.StartTime), TimeSpan.Parse(model.EndTime));
                    model.MaxDurationofSlab = Convert.ToDouble(NFhours);
                    model.MaxRateofSlab = model.NightFareAmount;
                }

                var ConvenienceFee = model.PaymentMode?.ToLower() == EPaymentMode.PayLater.ToString().ToLower() ? 0 : model.ConvenienceFee;

                var finalAmount = CalculateProratedAmount(model.StartDate, model.EndDate, model.FinalAmount);
                var totalAmount = CalculateProratedAmount(model.StartDate, model.EndDate, model.TotalAmount);
                var taxAmount = totalAmount * (model.TaxPercent / 100);
                var bookingAmount = finalAmount + taxAmount + model.ConvenienceFee;

                model.OverSizedCharges = model.OverSizedCharges ?? 0.00m;
                objCmd.Parameters.AddWithValue("@Id", model.Id);
                objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
                objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
                objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);
                objCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@IsEarlyBirdOfferApplied", model.IsEarlyBirdOfferApplied);
                objCmd.Parameters.AddWithValue("@IsNightFareOfferApplied", model.IsNightFareOfferApplied);
                objCmd.Parameters.AddWithValue("@EarlyBirdId", model.EarlyBirdId);
                objCmd.Parameters.AddWithValue("@NightFareId", model.NightFareId);

                objCmd.Parameters.AddWithValue("@BookingAmount", finalAmount);
                objCmd.Parameters.AddWithValue("@OverSizedCharges", model.OverSizedCharges);
                objCmd.Parameters.AddWithValue("@TaxAmount", taxAmount);
                objCmd.Parameters.AddWithValue("@FinalAmount", bookingAmount);
                objCmd.Parameters.AddWithValue("@IsPaymentFromCustomerSite", model.IsPaymentFromCustomerSite);
                objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);
                objCmd.Parameters.AddWithValue("@ConvenienceFee", ConvenienceFee);
                objCmd.Parameters.AddWithValue("@BookingCategoryId", model.BookingCategoryId);
                objCmd.Parameters.AddWithValue("@SubscriptionId", subscriptionId);
                objCmd.Parameters.AddWithValue("@BookingNotes", model.Notes);


                var Notes = (model.StartDate + StartTime).ToString("MMM dd h:mm tt") + " to " + (model.EndDate + EndTime).ToString("MMM dd h:mm tt");

                objCmd.Parameters.AddWithValue("@Notes", Notes);
                objCmd.Parameters.AddWithValue("@MaxDurationofSlab", model.MaxDurationofSlab);
                objCmd.Parameters.AddWithValue("@MaxRateofSlab", model.MaxRateofSlab);
                objCmd.Parameters.AddWithValue("@SendeTicket", (model.IsPaymentFromCustomerSite == true || model.PaymentMode.ToLower().Equals(EPaymentMode.Electronic.ToString().ToLower()) ? true : model.SendeTicket));

                if (model.GuestInfo != null)
                {
                    objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                    objCmd.Parameters.AddWithValue("@Email", model.Email);
                    objCmd.Parameters.AddWithValue("@NumberPlate", model.GuestInfo.NumberPlate);
                    objCmd.Parameters.AddWithValue("@VehicleModal", model.GuestInfo.VehicleModal);
                    objCmd.Parameters.AddWithValue("@VehicleTypeId", model.GuestInfo.VehicleTypeId);
                    objCmd.Parameters.AddWithValue("@VehicleColorId", model.GuestInfo.VehicleColorId);
                    objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.GuestInfo.VehicleManufacturerId);
                    objCmd.Parameters.AddWithValue("@StateCode", model.GuestInfo.StateCode);
                    objCmd.Parameters.AddWithValue("@CountryCode", model.GuestInfo.CountryCode);
                }
                objCmd.Parameters.AddWithValue("@IsCustomerAddRequired", model.IsCustomerAddRequired);
                objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
                objCmd.Parameters.AddWithValue("@IsFromQRScan", model.IsFromQRScan);
                objCmd.Parameters.AddWithValue("@PaymentGatewayCustomerId", model.PaymentGatewayCustomerId);
                objCmd.Parameters.AddWithValue("@PricingPlanId", model.SquareupInfo.PricingPlanId);
                objCmd.Parameters.AddWithValue("@FinalBookingAmount", model.FinalAmount);
                objCmd.Parameters.AddWithValue("@BookingDetailRef", MapDataTable.ToDataTable(lstBookingDetails));


                DataSet ds = objSQL.FetchDB(objCmd);

                PostBookingModel postBookingModel = null;

                if (ds.Tables[0].Rows.Count > 0)
                {

                    postBookingModel = new PostBookingModel();
                    var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
                    if (!string.IsNullOrEmpty(Error))
                        throw new AppException(Error);


                    postBookingModel = (from DataRow dr in ds.Tables[0].Rows
                                        select new PostBookingModel
                                        {
                                            BookingId = Convert.ToInt64(dr["BookingId"]),
                                            CustomerId = Convert.ToInt64(dr["CustomerId"]),
                                            NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                            Symbol = Convert.ToString(dr["Symbol"]),
                                            LocationName = Convert.ToString(dr["LocationName"])
                                        }).FirstOrDefault();


                    postBookingModel.BadgeCounts = (from DataRow dr in ds.Tables[0].Rows
                                                    select new BadgeCount
                                                    {
                                                        CustomerBadgeCount = Convert.ToInt64(dr["CustomerBadgeCount"]),
                                                        ValetBadgeCount = Convert.ToInt64(dr["ValetBadgeCount"])
                                                    }).FirstOrDefault();

                    DeviceToken devicetokens = new DeviceToken();

                    devicetokens.StaffTokens = (from DataRow dr in ds.Tables[1].Rows
                                                select Convert.ToString(dr["DeviceToken"])
                                                    ).ToList();
                    // postBookingModel.DeviceTokens.CustomerTokens
                    devicetokens.CustomerTokens = (from DataRow dr in ds.Tables[2].Rows
                                                   select new CustomerToken
                                                   {
                                                       DeviceToken = Convert.ToString(dr["DeviceToken"]),
                                                       BrowserDeviceToken = Convert.ToString(dr["BrowserDeviceToken"])
                                                   }).FirstOrDefault();
                    postBookingModel.DeviceTokens = devicetokens;

                    postBookingModel.ListOwnerSupervisors = (from DataRow dr in ds.Tables[3].Rows
                                                             select Convert.ToString(dr["Email"])
                                                  ).ToList();
                }
                return postBookingModel;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
        public string GetLocationStaticQRCode(long ParkingLocationId)
		{
			try
			{
				string url = "https://flixvalet.com/home";
				return _qRRepo.GetStaticTigerQRImage(url);
			}
			catch
			{
				throw;
			}

		}

		public QRListResponse GetLocationQRList(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, bool isMonthly = false)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetQRListForOwnerLocations");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@IsMonthly", isMonthly);

                DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new LocationQRData
										{
											Id = Convert.ToInt64(dr["Id"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											QRCodePath = Convert.ToString(dr["QRCodePath"])
										}).ToList();

					return new QRListResponse { ListQrs = lstlocations, Total = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]) };
				}
				return null;
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

		public List<LocationQRData> GetOwnerLocationsWithoutQR(long ParkingBusinessOwnerId, bool isMonthly = false)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetOwnerLocationsWithoutQR");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@IsMonthly", isMonthly);

                DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new LocationQRData
										{
											Id = Convert.ToInt64(dr["Id"]),
											LocationName = Convert.ToString(dr["LocationName"])
										}).ToList();

					return lstlocations;
				}
				return null;

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

		public string GenerateDynamicQR(DynamicQRCodeModel model)
		{
			string QRCode = string.Empty;
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateLocationQR");
			try
			{
				
				string url = model.IsMonthly ? $"https://flixvalet.com/monthlyqr?redirect_url=home/BookParking/{model.ParkingLocationId}" : $"https://flixvalet.com/qr?redirect_url=home/BookParking/{model.ParkingLocationId}"; 

				QRCode = _qRRepo.GetDynamicTigerQRImage(url, model.LogoUrl ?? _appsettings.LogoUrl);

				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@QRCodePath", QRCode);
                objCmd.Parameters.AddWithValue("@IsMonthly", model.IsMonthly);

                objSQL.UpdateDB(objCmd, true);

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

			return QRCode;
		}

		public BookingExtensionPendingResponse GetBookingExtensionPendingList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string CurrentDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingExtensionPendingList");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

				DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstBookings = (from DataRow dr in dtLocations.Rows
									   select new ExtensionPendingList
									   {
										   BookingId = Convert.ToInt64(dr["BookingId"]),
										   CustomerName = Convert.ToString(dr["CustomerName"]),
										   TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
										   StartDate = Convert.ToDateTime(dr["StartDate"]),
										   EndDate = Convert.ToDateTime(dr["EndDate"]),
										   NumberPlate = Convert.ToString(dr["NumberPlate"])
									   }).ToList();

					return new BookingExtensionPendingResponse { Bookings = lstBookings, Total = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]) };
				}
				return null;
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

		public BookingsByOwnerResponse BookingRevenueReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string BookingType, string StartDate, string EndDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("BookingRevenueReportv1");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@BookingType", BookingType);
				objCmd.Parameters.AddWithValue("@StartDate", StartDate);
				objCmd.Parameters.AddWithValue("@EndDate", EndDate);

				DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new BookingList
										{
											BookingId = Convert.ToInt64(dr["BookingId"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											CustomerName = Convert.ToString(dr["CustomerName"]),
											TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
											StartDate = Convert.ToDateTime(dr["StartDate"]),
											EndDate = Convert.ToDateTime(dr["EndDate"]),
											StartTime = Convert.ToString(dr["StartTime"]),
											EndTime = Convert.ToString(dr["EndTime"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											BookingType = Convert.ToString(dr["BookingType"]),
											UnpaidAmount = Convert.ToDecimal(dr["UnpaidAmount"]),
											ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
										}).ToList();

					return new BookingsByOwnerResponse
					{
						Bookings = lstlocations,
						Total = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]),
						HourlyRevenue = Convert.ToDecimal(dtLocations.Rows[0]["HourlyRevenue"]),
						MonthlyRevenue = Convert.ToDecimal(dtLocations.Rows[0]["MonthlyRevenue"]),
						TotalMonthlyBookings = Convert.ToInt32(dtLocations.Rows[0]["MonthlyBookings"]),
						TotalHourlyBookings = Convert.ToInt32(dtLocations.Rows[0]["HourlyBookings"]),
						TotalConvenienceFee = Convert.ToDecimal(dtLocations.Rows[0]["TotalConvenienceFee"])
                    };
				}
				return new BookingsByOwnerResponse();
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

        public BookingsByOwnerResponse GetBookingRevenueCSVReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string BookingType, string StartDate, string EndDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingRevenueCSVReport");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@BookingType", BookingType);
                objCmd.Parameters.AddWithValue("@StartDate", StartDate);
                objCmd.Parameters.AddWithValue("@EndDate", EndDate);

                DataTable dtLocations = objSQL.FetchDT(objCmd);
                if (dtLocations.Rows.Count > 0)
                {
                    var lstlocations = (from DataRow dr in dtLocations.Rows
                                        select new BookingList
                                        {
                                            BookingId = Convert.ToInt64(dr["BookingId"]),
                                            LocationName = Convert.ToString(dr["LocationName"]),
                                            CustomerName = Convert.ToString(dr["CustomerName"]),
                                            TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
                                            StartDate = Convert.ToDateTime(dr["StartDate"]),
                                            EndDate = Convert.ToDateTime(dr["EndDate"]),
                                            StartTime = Convert.ToString(dr["StartTime"]),
                                            EndTime = Convert.ToString(dr["EndTime"]),
                                            NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                            BookingType = Convert.ToString(dr["BookingType"]),
                                            UnpaidAmount = Convert.ToDecimal(dr["UnpaidAmount"]),
                                            ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
                                        }).ToList();

                    return new BookingsByOwnerResponse
                    {
                        Bookings = lstlocations,
                        HourlyRevenue = Convert.ToDecimal(dtLocations.Rows[0]["HourlyRevenue"]),
                        MonthlyRevenue = Convert.ToDecimal(dtLocations.Rows[0]["MonthlyRevenue"]),
                        TotalMonthlyBookings = Convert.ToInt32(dtLocations.Rows[0]["MonthlyBookings"]),
                        TotalHourlyBookings = Convert.ToInt32(dtLocations.Rows[0]["HourlyBookings"]),
                        TotalConvenienceFee = Convert.ToDecimal(dtLocations.Rows[0]["TotalConvenienceFee"])
                    };
                }
                return new BookingsByOwnerResponse();
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

        public AccountReceivableReportResponse AccountReceivableReport(long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BusinessOffice, string StartDate, string EndDate, long BookingCategoryId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AccountReceivableReport_v2");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@BookingCategoryId", BookingCategoryId);
				objCmd.Parameters.AddWithValue("@BusinessOffice", BusinessOffice);
				objCmd.Parameters.AddWithValue("@StartDate", StartDate);
				objCmd.Parameters.AddWithValue("@EndDate", EndDate);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);


				DataTable dtLocations = objSQL.FetchDT(objCmd);
				if (dtLocations.Rows.Count > 0)
				{
					var lstlocations = (from DataRow dr in dtLocations.Rows
										select new BookingDetailList
										{
											BookingId = Convert.ToInt64(dr["BookingId"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											CustomerName = Convert.ToString(dr["CustomerName"]),
											TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
											StartDate = Convert.ToDateTime(dr["StartDate"]),
											EndDate = Convert.ToDateTime(dr["EndDate"]),
											StartTime = Convert.ToString(dr["StartTime"]),
											EndTime = Convert.ToString(dr["EndTime"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											BookingType = Convert.ToString(dr["BookingType"]),
											UnpaidAmount = Convert.ToDecimal(dr["UnpaidAmount"]),
											CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
											TimeZoneId = Convert.ToInt64(dr["TimeZoneId"])
										}).ToList();

					return new AccountReceivableReportResponse
					{
						Bookings = lstlocations,
						TotalBookings = Convert.ToInt32(dtLocations.Rows[0]["TotalCount"]),
						TotalAmount = Convert.ToDecimal(dtLocations.Rows[0]["AmountTotal"])
					};
				}
				return new AccountReceivableReportResponse();
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

		public long AddParkingOwnerBusinessOffice(ParkingOwnerBusinessOffices model, string origin = "")
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddParkingOwnerBusinessOffice_v2");

			try
			{
                //var password = PasswordGenerator.GenerateRandomPassword();
                var emailUsername = model.Email.Split('@')[0];
                var password = $"{emailUsername}@123";
                var passwordHash = BC.HashPassword(password);
				objCmd.Parameters.AddWithValue("@BusinessOfficeId", model.BusinessOfficeId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@Name", model.Name);
				objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@Password", passwordHash);
                objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
				objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
				objCmd.Parameters.AddWithValue("@ModifyBy", model.ModifyBy);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				string Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				if (model.BusinessOfficeId <= 0)
				{
					if (!string.IsNullOrEmpty(model.Email))
						sendRegistrationEmail(model.Email, password, origin);
				}

				return Convert.ToInt64(dtCustomer.Rows[0]["OfficeId"]);
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

        private void sendRegistrationEmail(string email, string password, string origin = "")
        {
            string MailText = PasswordGenerator.GetEmailTemplateText("\\wwwroot\\EmailTemplates\\RegistrationEmailForBusinessOffice.html");
            MailText = string.Format(MailText, _appsettings.LoginUrl, email, password, _appsettings.AppName);
            _emailService.Send(
            to: email,
            subject: $"Registration successful for {_appsettings.AppName}",
                html: $@"{MailText}"
            );
        }

        public List<POBusinessOfficeListResponse> GetPOBusinessOfficeList(long ParkingLocationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBusinessOfficeList");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                DataTable dtCustomer = objSQL.FetchDT(objCmd);

                if (dtCustomer.Rows.Count > 0)
                {
                    var list = (from DataRow dr in dtCustomer.Rows
                                select new POBusinessOfficeListResponse
                                {
                                    BusinessOfficeId = Convert.ToInt64(dr["Id"]),
                                    ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                    Name = Convert.ToString(dr["Name"]),
                                    LocationName = Convert.ToString(dr["LocationName"]),
                                    IsActive = Convert.ToBoolean(dr["IsActive"])
                                }).ToList();

					return list;
                }
                return null;
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

        public POBusinessOfficeList GetAllPOBusinessOfficeList(long? ParkingBusinessOwnerId, long? ParkingLocationId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBusinessOfficeList_v1");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@PageNo", PageNo);
                objCmd.Parameters.AddWithValue("@PageSize", PageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", SortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", SortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);

                DataTable dtCustomer = objSQL.FetchDT(objCmd);

                if (dtCustomer.Rows.Count > 0)
                {
                    var list = (from DataRow dr in dtCustomer.Rows
                                select new POBusinessOfficeListResponse
                                {
                                    BusinessOfficeId = Convert.ToInt64(dr["Id"]),
                                    ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                    Name = Convert.ToString(dr["Name"]),
                                    LocationName = Convert.ToString(dr["LocationName"]),
                                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                                    UserId = Convert.ToInt64(dr["UserId"])
                                }).ToList();


                    return new POBusinessOfficeList { Offices = list, Total = Convert.ToInt32(dtCustomer.Rows[0]["TotalCount"]) };
                }
                return null;
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


        public POBusinessOfficeListResponse GetPOBusinessOfficeById(long BusinessOfficeId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetPOBusinessOfficeById");
			try
			{
				objCmd.Parameters.AddWithValue("@BusinessOfficeId", BusinessOfficeId);

				DataTable dtOffice = objSQL.FetchDT(objCmd);

				var office = (from DataRow dr in dtOffice.Rows
							  select new POBusinessOfficeListResponse
							  {
								  BusinessOfficeId = Convert.ToInt64(dr["Id"]),
								  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
								  Name = Convert.ToString(dr["Name"]),
								  IsActive = Convert.ToBoolean(dr["IsActive"])
							  }).FirstOrDefault();
				return office;
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

		public long ChangeBusinessOfficeActiveStatus(BusinessOfficeIdModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_ChangeBusinessOfficeActiveStatus");
			try
			{
				objCmd.Parameters.AddWithValue("@BusinessOfficeId", model.BusinessOfficeId);
				DataTable dtOffice = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtOffice.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.BusinessOfficeId;
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
		
		public List<RelayTypes> GetRelayTypes()
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetRelayTypes");
			try
			{
				DataTable dtRelayTypes = objSQL.FetchDT(objCmd);

				var relayTypes = (from DataRow dr in dtRelayTypes.Rows
								 select new RelayTypes
								 {
									 Id = Convert.ToInt32(dr["Id"]),
									 Name = Convert.ToString(dr["Name"])
								 }).ToList();

				return relayTypes;
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

		public ParkingLocationGateSettingResponse GetAllParkingLocationGateSettings(long ParkingBusinessOwnerId, int pageNo, int? pageSize, string sortColumn, string sortOrder, string searchValue)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetAllParkingLocationGateSettings");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", searchValue);

				DataTable dtLocationGateSettings = objSQL.FetchDT(objCmd);
				if (dtLocationGateSettings.Rows.Count > 0)
				{
					var lstLocationGateSettings = (from DataRow dr in dtLocationGateSettings.Rows
										select new LocationGateSettingList
										{
											Id = Convert.ToInt64(dr["Id"]),
											ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											RelayName = Convert.ToString(dr["RelayName"]),
											RelayURL = Convert.ToString(dr["RelayURL"]),
											QueueURL = Convert.ToString(dr["QueueURL"]),
											QueueName = Convert.ToString(dr["QueueName"]),
											QRCodePath = Convert.ToString(dr["QRCodePath"])
										}).ToList();

					return new ParkingLocationGateSettingResponse { ParkingLocationsGateSettings = lstLocationGateSettings, Total = Convert.ToInt32(dtLocationGateSettings.Rows[0]["TotalCount"]) };
				}
				return null;
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

		public ParkingLocationGateSettings GetParkingLocationGateSettingById(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationGateSettingById");
			try
			{
				objCmd.Parameters.AddWithValue("@Id", Id);

				DataTable dtLocationGateSetting = objSQL.FetchDT(objCmd);

				var locationGateSetting = (from DataRow dr in dtLocationGateSetting.Rows
							  select new ParkingLocationGateSettings
							  {
								  Id = Convert.ToInt64(dr["Id"]),
								  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),   
								  RelayTypeId = Convert.ToInt32(dr["RelayTypeId"]),
								  RelayName = Convert.ToString(dr["RelayName"]),
								  RelayURL = Convert.ToString(dr["RelayURL"]),
								  QueueURL = Convert.ToString(dr["QueueURL"]),
								  QueueName = Convert.ToString(dr["QueueName"]),
								  Region = Convert.ToString(dr["Region"]),
								  GateNumber = Convert.ToInt32(dr["GateNumber"]),
								  IsEnter = Convert.ToBoolean(dr["IsEnter"]),
								  QRCodePath = Convert.ToString(dr["QRCodePath"])
							  }).FirstOrDefault();
				return locationGateSetting;
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


		public long AddPakingLocationGateSettings(ParkingLocationGateSettings model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddParkingLocationGateSetting");
			try
			{
				string QueueName = string.Empty;
				var (gateNumber,locationPic) = FetchGateNumberByParkingLocationId(model.ParkingLocationId);
				if(model.Id == 0)
				{
					if(gateNumber == 0)
						gateNumber = 1;
					else
						gateNumber = gateNumber + 1;
					
					string locationId  = model.ParkingLocationId.ToString();
					QueueName = $"{locationId}-{gateNumber}";
					model.QueueURL = _aWSQueueService.CreateQueue(QueueName).Result;
				}
				
				objCmd.Parameters.AddWithValue("@Id", model.Id);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@RelayTypeId", model.RelayTypeId);
				objCmd.Parameters.AddWithValue("@RelayName", model.RelayName);
				objCmd.Parameters.AddWithValue("@RelayURL", model.RelayURL);
				objCmd.Parameters.AddWithValue("@QueueName", QueueName);
				objCmd.Parameters.AddWithValue("@QueueURL", model.QueueURL);
				objCmd.Parameters.AddWithValue("@Region", _aWSSQSDetails.Region);
				objCmd.Parameters.AddWithValue("@IsEnter", model.IsEnter);
				objCmd.Parameters.AddWithValue("@GateNumber", gateNumber);
				DataTable dtLocation = objSQL.FetchDT(objCmd);

				long gateSettingId = Convert.ToInt64(dtLocation.Rows[0]["Id"]);
				if(model.Id == 0)
					UpdateLocationGateQRCode(gateSettingId, model.ParkingLocationId, locationPic);
				return gateSettingId;
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

		public long DeleteParkingLocationGateSetting(CommonId model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_DeleteParkingLocationGateSettings");
			try
			{
				var gateSettingDetails = GetParkingLocationGateSettingById(model.Id);
				if(gateSettingDetails != null)
					_aWSQueueService.DeleteQueue(gateSettingDetails.QueueURL);

				objCmd.Parameters.AddWithValue("@Id", model.Id);
				DataTable dtLoc = objSQL.FetchDT(objCmd);
				var Error = Convert.ToString(dtLoc.Rows[0]["Error"]);

				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.Id;
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

		public ParkingLocationGateSettings GetLocationGateSettingByLocationId(long parkingLocationId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetLocationGateSettingByLocationId");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", parkingLocationId);

				DataTable dtLocationGateSetting = objSQL.FetchDT(objCmd);

				var locationGateSetting = (from DataRow dr in dtLocationGateSetting.Rows
										   select new ParkingLocationGateSettings
										   {
											   Id = Convert.ToInt64(dr["Id"]),
											   ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											   RelayName = Convert.ToString(dr["RelayName"]),
											   RelayURL = Convert.ToString(dr["RelayURL"]),
											   QueueURL = Convert.ToString(dr["EnterQueueURL"]),
											   IsEnter = Convert.ToBoolean(dr["IsEnter"]),
										   }).FirstOrDefault();
				return locationGateSetting;
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

		public void OpenCloseGate(long Id)
		{
			var gateSettings = GetParkingLocationGateSettingById(Id);

			if (gateSettings != null)
			{
				string message = string.Empty;
				message = $"open/{gateSettings.RelayName}";
				_aWSQueueService.SendMessage(gateSettings.QueueURL, message);
			}
		}

		public List<ParkingLocationName> GetParkingGateSettingLocationList(long ParkingBusinessOwnerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkingGateSettingLocationList");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
				
				DataTable dtLocations = objSQL.FetchDT(objCmd);

				var locations = (from DataRow dr in dtLocations.Rows
								 select new ParkingLocationName
								 {
									 Id = Convert.ToInt32(dr["Id"]),
									 LocationName = Convert.ToString(dr["LocationName"])
								 }).ToList();

				return locations;
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
		
		public (long, string) FetchGateNumberByParkingLocationId(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_FetchGateNumberByParkingLocationId");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", Id);

				DataTable dtLocationGateSetting = objSQL.FetchDT(objCmd);

				var locationGateSetting = (from DataRow dr in dtLocationGateSetting.Rows
							  select new 
							  {
								  GateNumber = dr["GateNumber"] == DBNull.Value ? 0 : Convert.ToInt32(dr["GateNumber"]),
								  LocationPic = Convert.ToString(dr["LocationPic"])
							  }).FirstOrDefault();
				if(locationGateSetting != null)			  
					return (locationGateSetting.GateNumber , locationGateSetting.LocationPic);
				else
					return(0, string.Empty);
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
		
		public void UpdateLocationGateQRCode(long Id, long ParkingLocationId, string LogoUrl)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateLocatiomGateQRCode");
			try
			{
				string url = $"https://flixvalet.com/qr?redirect_url=home/BookParking/{ParkingLocationId}/{Id}"; // to be replaced later.
				string QRCodePath = _qRRepo.GetDynamicTigerQRImage(url, LogoUrl ?? _appsettings.LogoUrl);
				objCmd.Parameters.AddWithValue("@Id", Id);
				objCmd.Parameters.AddWithValue("@QRCodePath", QRCodePath);
				DataTable dt = objSQL.FetchDT(objCmd);
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

        public List<ParkingLocationGateNumbersResponse> GetParkingLocationGates(long ParkingLocationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationGates");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);

                DataTable dtLocations = objSQL.FetchDT(objCmd);

				var gateNumbers = (from DataRow dr in dtLocations.Rows
								   select new ParkingLocationGateNumbersResponse
								   {
									   Id = Convert.ToInt32(dr["Id"]),
									   GateNumber = "Gate-" + dr["GateNumber"].ToString()
								   }).ToList();
			
                return gateNumbers;
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

        public ParkingLocDetailsResponse GetMonthlyQRBookingInfo(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, bool IsFullTimeBooking, long? CustomerVehicleId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEstimatedBookingDetailsByLoc");
            try
            {
                string bookingType = "monthly";
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                objCmd.Parameters.AddWithValue("@BookingType", bookingType);
           
                DataSet ds = objSQL.FetchDB(objCmd);

                var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);


                var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

                StatesMst state = null; Countries country = null;

                var plocation = (from DataRow dr in ds.Tables[1].Rows
                                 select new ParkingLocDetailsResponse
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     LocationName = Convert.ToString(dr["LocationName"]),
                                     Address = dr["Address"].ToString(),
                                     City = dr["City"].ToString(),
                                     State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
                                     StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
                                     Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
                                     CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
                                     ZipCode = dr["ZipCode"].ToString(),
                                     LocationPic = dr["LocationPic"].ToString(),
                                     Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                     MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                     OverSizedChargesMonthly = Convert.ToDecimal(dr["OverSizedChargesMonthly"]),
                                     OverSizedChargesRegular = Convert.ToDecimal(dr["OverSizedChargesRegular"]),
                                     PaymentMethod = dr["PaymentGateway"] == DBNull.Value ? null : Convert.ToString(dr["PaymentGateway"]),
                                     ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
                                     SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
                                     AccessToken = dr["AccessToken"] == DBNull.Value ? null : Convert.ToString(dr["AccessToken"]),
                                     IsProduction = dr["IsProduction"] == DBNull.Value ? false : Convert.ToBoolean(dr["IsProduction"]),
                                     ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
                                     LocationId = dr["LocationId"] == DBNull.Value ? null : Convert.ToString(dr["LocationId"]),
                                     TaxPercent = Convert.ToDecimal(dr["Tax"]),
                                     Currency = Convert.ToString(dr["Currency"]),
                                     Symbol = Convert.ToString(dr["Symbol"]),
                                     BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
                                     LogoUrl = Convert.ToString(dr["LogoUrl"]),
                                     ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
                                 }).FirstOrDefault();

                if (plocation == null)
                    throw new AppException("Location not found");

                var timings = (from DataRow dr in ds.Tables[2].Rows
                               select new ParkingLocationTiming
                               {
                                   IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                   IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                   IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                   IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                   IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                   IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                   IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                   StartDate = Convert.ToDateTime(dr["StartDate"]),
                                   StartDateUtc = Convert.ToDateTime(dr["StartDateUtc"]),
                                   StartTime = TimeSpan.Parse(dr["StartTime"].ToString()),
                                   EndTime = TimeSpan.Parse(dr["EndTime"].ToString())
                               }).ToList();

                plocation.ParkingTimings = parkingHelper.GetTimings(timings);
                int daysInMonth = DateTime.DaysInMonth(StartDate.Year, StartDate.Month);

                var rates = (from DataRow dr in ds.Tables[3].Rows
                             select new ParkingLocationRateRequest
                             {
                                 Duration = Convert.ToInt32(dr["DurationUpto"]) * daysInMonth,
                                 Charges = Convert.ToDecimal(dr["Rate"])
                             }).ToList();

                if (rates == null || rates.Count == 0)
                    throw new AppException("Rates aren't specified for the provided location.");

                var (TotalHours, PerHourRate, rate) = parkingHelper.GetTotalHoursandAmountByDuration(rates, DateTime.Parse(StartDate.ToShortDateString()), DateTime.Parse(EndDate.ToShortDateString()), StartTime, EndTime, IsFullTimeBooking);


                decimal TotalAmount = IsFullTimeBooking ? rate.Charges : parkingHelper.RoundOff(PerHourRate * TotalHours);


                plocation.PerHourRate = PerHourRate;
                plocation.TotalHours = Math.Round(TotalHours);
                plocation.TotalAmount = TotalAmount;
                plocation.FinalAmount = TotalAmount;
                plocation.OverSizedCharges = plocation.OverSizedCharges ?? 0.00m;
                plocation.MaxDurationofSlab = rate.Duration;
                plocation.MaxRateofSlab = rate.Charges;
                var CustInfo = (from DataRow dr in ds.Tables[4].Rows
                                select new
                                {
                                    Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
                                    CustomerId = Convert.ToInt64(dr["CustomerId"]),
                                    NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                    VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
                                    Address = Convert.ToString(dr["Address"]),
                                    VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["VehicleTypeId"])
                                }).ToList();

                if (CustInfo != null && CustInfo.Count > 0)
                {
                    if (CustomerVehicleId != null && CustInfo.Find(a => a.Id.Equals(CustomerVehicleId) && a.VehicleTypeId.Equals(2)) != null)
                    {
                        plocation.OverSizedCharges = plocation.OverSizedChargesMonthly;
                        plocation.IsOverSizedVehicle = true;
                        plocation.FinalAmount += Convert.ToDecimal(plocation.OverSizedCharges);

                    }
                    plocation.CustomerDetails = new CustomerDetails
                    {
                        Address = CustInfo.Select(a => a.Address).FirstOrDefault(),
                        CustomerId = CustInfo.Select(a => a.CustomerId).FirstOrDefault(),
                        CustomerVehicles = CustInfo.FirstOrDefault().Id != null ?
                    CustInfo.AsEnumerable().Select(a => new VehicleDetails
                    {
                        Id = a.Id,
                        NumberPlate = a.NumberPlate,
                        VehicleModal = a.VehicleModal
                    }).Where(e => e.NumberPlate.ToLower() != "unknown").ToList() : null
                    };
                }
                plocation.FinalAmount += plocation.ConvenienceFee;
                plocation.TaxAmount = parkingHelper.RoundOff((plocation.TaxPercent > 0.00m ? ((plocation.FinalAmount * plocation.TaxPercent) / 100) : 0.00m));
                plocation.FinalAmount += plocation.TaxAmount;


                // early bird check
                if (StartDate == EndDate)
                {
                    var earlyBirdOffer = (from DataRow dr in ds.Tables[5].Rows
                                          select new ParkingLocationEarlyBirdOffer
                                          {
                                              Id = Convert.ToInt64(dr["Id"]),
                                              IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                              IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                              IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                              IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                              IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                              IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                              IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                              ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                              Amount = Convert.ToDecimal(dr["Amount"]),
                                              EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
                                              EnterToTime = Convert.ToString(dr["EnterToTime"]),
                                              ExitByTime = Convert.ToString(dr["ExitByTime"]),
                                              IsActive = Convert.ToBoolean(dr["IsActive"])
                                          }).FirstOrDefault();

                    var earlyBirdInfo = parkingHelper.GetEarlyBirdInfo(earlyBirdOffer, StartDate, StartTime
                    , EndTime);
                    plocation.IsEarlyBirdOfferApplicable = earlyBirdInfo == null ? false : true;

                    plocation.EarlyBirdFinalAmount = plocation.IsEarlyBirdOfferApplicable ? earlyBirdInfo.Amount + plocation.OverSizedCharges + plocation.ConvenienceFee : (decimal?)null;
                    plocation.EarlyBirdTaxAmount = parkingHelper.RoundOff(plocation.TaxPercent > 0.00m ? ((plocation.EarlyBirdFinalAmount * plocation.TaxPercent) / 100) : 0.00m);
                    plocation.EarlyBirdFinalAmount += plocation.EarlyBirdTaxAmount;
                    plocation.EarlyBirdInfo = earlyBirdInfo;
                }

                var nightFareOffer = (from DataRow dr in ds.Tables[6].Rows
                                      select new ParkingLocationNightFareOffer
                                      {
                                          Id = Convert.ToInt64(dr["Id"]),
                                          IsMonday = Convert.ToBoolean(dr["IsMonday"]),
                                          IsTuesday = Convert.ToBoolean(dr["IsTuesday"]),
                                          IsWednesday = Convert.ToBoolean(dr["IsWednesday"]),
                                          IsThursday = Convert.ToBoolean(dr["IsThursday"]),
                                          IsFriday = Convert.ToBoolean(dr["IsFriday"]),
                                          IsSaturday = Convert.ToBoolean(dr["IsSaturday"]),
                                          IsSunday = Convert.ToBoolean(dr["IsSunday"]),
                                          ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                          Amount = Convert.ToDecimal(dr["Amount"]),
                                          EnterFromTime = Convert.ToString(dr["EnterFromTime"]),
                                          EnterToTime = Convert.ToString(dr["EnterToTime"]),
                                          ExitByTime = Convert.ToString(dr["ExitByTime"]),
                                          IsActive = Convert.ToBoolean(dr["IsActive"])
                                      }).FirstOrDefault();

                var nightFareInfo = parkingHelper.GetNightFareInfo(nightFareOffer, StartDate, EndDate, StartTime, EndTime);

                plocation.IsNightFareOfferApplicable = nightFareInfo == null ? false : true;

                plocation.NightFareFinalAmount = plocation.IsNightFareOfferApplicable ? nightFareInfo.Amount + plocation.OverSizedCharges + plocation.ConvenienceFee : (decimal?)null;
                plocation.NightFareTaxAmount = parkingHelper.RoundOff(plocation.TaxPercent > 0.00m ? ((plocation.NightFareFinalAmount * plocation.TaxPercent) / 100) : 0.00m);
                plocation.NightFareFinalAmount += plocation.NightFareTaxAmount;
                plocation.NightFareInfo = nightFareInfo;

                plocation.StartDate = StartDate;
                plocation.EndDate = EndDate;
                plocation.StartTime = StartTime;
                plocation.EndTime = EndTime;


                var minutes = (int)((plocation.TotalHours - Math.Truncate(plocation.TotalHours)) * 60);
                var duration = (int)plocation.TotalHours + " hours" + (minutes == 0 ? "" : " " + minutes + " minutes");
                plocation.MaxStay = duration;
                return plocation;
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

        public POBusinessOfficeListResponse GetBusinessOfficeByUserId(long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBusinessOfficeByUserId");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);

                DataTable dtOffice = objSQL.FetchDT(objCmd);

                var office = (from DataRow dr in dtOffice.Rows
                                           select new POBusinessOfficeListResponse
                                           {
                                               BusinessOfficeId = Convert.ToInt64(dr["Id"]),
                                               ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                               Name = Convert.ToString(dr["Name"]),
                                               LocationName = Convert.ToString(dr["LocationName"]),
                                               IsActive = Convert.ToBoolean(dr["IsActive"])
                                           }).FirstOrDefault();
                return office;
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

        public ChargeBackBookingList ChargeBackCustomerBookingReport(long businessOfficeId, int pageNo, int? pageSize, string sortColumn, string sortOrder)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ChargeBackCustomerBookingReport");
            try
            {
                objCmd.Parameters.AddWithValue("@BusinessOfficeId", businessOfficeId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);

                DataTable dtReports = objSQL.FetchDT(objCmd);
                if (dtReports.Rows.Count > 0)
                {
                    var reports = (from DataRow dr in dtReports.Rows
                                    select new ChargeBackBookingReport
                                    {   
										Id = Convert.ToInt64(dr["Id"]),
                                        Url = Convert.ToString(dr["Url"]),
                                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]).AddDays(-1),
                                    }).ToList();

                    return new ChargeBackBookingList { List = reports, Total = Convert.ToInt32(dtReports.Rows[0]["TotalCount"])};
                }
                return null;
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

    //    public QrcodeScanDataResponse GetQrCodeActivitylogs(string qrId, string timeZone, string date)
    //    {
    //        try
    //        {
				//var dateTime = !string.IsNullOrEmpty(date)? DateTime.ParseExact(date, "yyyy-MM-dd", null) : DateTime.UtcNow;

				//var response = _qRRepo.GetQrCodeScanData(qrId, timeZone, dateTime);

				//var list = new List<QrcodeScanData>();
				//int totalscans = 0;
				//if (response != null && response.Data != null)
				//{
    //                list = response.Data.Data?.Select(res => new QrcodeScanData
    //                {
    //                    City = res.Id.City,
    //                    CountryCode = res.Id.Loc,
    //                    DeviceType = res.Id.Device,
    //                    Scans = res.Count
    //                }).ToList() ?? new List<QrcodeScanData>();

    //                totalscans = response.Data.Scans;  
				//}
    //            return new QrcodeScanDataResponse() { List = list, Total = response.Data.Scans };
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }

		public QrcodeScanDataResponse GetQrCodeActivitylogs(long parkingLocationId, string bookingType, string date)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationById");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", parkingLocationId);

				DataTable dtLocation = objSQL.FetchDT(objCmd);
				var location = new ParkingLocationBasicDetails();
				if (dtLocation.Rows.Count > 0)
				{
					location = (from DataRow dr in dtLocation.Rows
								select new ParkingLocationBasicDetails
								{
									Id = Convert.ToInt64(dr["Id"]),
									LocationName = Convert.ToString(dr["LocationName"]),
									QrCodePath = Convert.ToString(dr["QrCodePath"]),
									TimeZone = Convert.ToString(dr["TimeZone"]),
									QRCodePathMonthly = Convert.ToString(dr["QRCodePathMonthly"]),
								}).FirstOrDefault();
				}

				var list = new List<QrcodeScanData>();
				int totalscans = 0;
                if (location != null && (!string.IsNullOrEmpty(location.QrCodePath) || !string.IsNullOrEmpty(location.QRCodePathMonthly)))
                {
					var qrCodePath = bookingType.ToLower() == "monthly" ? location.QRCodePathMonthly : location.QrCodePath;
					string qrId = Path.GetFileNameWithoutExtension(qrCodePath);
					var dateTime = !string.IsNullOrEmpty(date) ? DateTime.ParseExact(date, "yyyy-MM-dd", null) : DateTime.UtcNow;

					var response = _qRRepo.GetQrCodeScanData(qrId, location.TimeZone, dateTime);


					if (response != null && response.Data != null)
					{
						list = response.Data.Data?.Select(res => new QrcodeScanData
						{
							City = res.Id.City,
							CountryCode = res.Id.Loc,
							DeviceType = res.Id.Device,
							Scans = res.Count
						}).ToList() ?? new List<QrcodeScanData>();

						totalscans = response.Data.Scans;
					}
				}
				return new QrcodeScanDataResponse() { List = list, Total = totalscans };
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

        public static decimal CalculateProratedAmount(DateTime startDate, DateTime endDate, decimal fullMonthAmount)
        {
            // Total days in the selected period
            int daysInPeriod = (endDate - startDate).Days + 1;

            // Get the total days in the full month based on the start date
            int daysInFullMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

            // Calculate the prorated amount
            decimal proratedAmount = (daysInPeriod / (decimal)daysInFullMonth) * fullMonthAmount;

            proratedAmount = Math.Ceiling(proratedAmount * 100) / 100;

            return proratedAmount;
        }
    }

}



