using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL;
using ValetParkingDAL.Models.UserModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Enums;
using System.Threading.Tasks;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingBLL.Helpers;
using ValetParkingAPI.Models;
using ValetParkingDAL.Models.PaymentModels.cs;
using System.Text.Json;
using CancelBookingModel = ValetParkingDAL.Models.CustomerModels.CancelBookingRequest;
using Square.Models;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Repository
{
	public class CustomerRepo : ICustomer
	{
		private readonly IConfiguration _configuration;
		private readonly IFirebase _firebaseRepo;
		// private SQLManager objSQL;
		private SqlCommand g_ObjSQLCmd;
		private string g_ConStr = string.Empty;
		private SqlConnection g_ObjCon;
		private readonly AppSettings _appSettings;
		private readonly IEmail _emailService;
		private readonly ISMS _smsService;
		private readonly ISquare _squareRepo;
		private readonly IStripe _stripeRepo;
		private readonly IPaypal _paypalRepo;
		private readonly IMapper _mapper;
		private readonly ParkingHelper parkingHelper;
		private readonly ICache _cacheRepo;
		private readonly DateTimeHelper _dateTimeHelper;
		private readonly IQR _qRRepo;
		private readonly IAWSService _aWSService;

		public CustomerRepo(IConfiguration configuration, IFirebase firebaseRepo, IEmail emailService, ISMS smsService, IMapper mapper, ISquare squareRepo, IStripe stripeRepo, IPaypal paypalRepo, ParkingHelper ParkingHelper, ICache cacheRepo, DateTimeHelper dateTimeHelper, IQR QRRepo, IAWSService aWSService)
		{
			_configuration = configuration;
			_firebaseRepo = firebaseRepo;
			_emailService = emailService;
			_smsService = smsService;
			_squareRepo = squareRepo;
			_stripeRepo = stripeRepo;
			_paypalRepo = paypalRepo;
			// _mapper = mapper;
			_appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
			parkingHelper = ParkingHelper;
			_cacheRepo = cacheRepo;
			_dateTimeHelper = dateTimeHelper;
			_qRRepo = QRRepo;
			_aWSService = aWSService;

			var config = new MapperConfiguration(cfg =>
	   {
		   cfg.CreateMap<AdditionalPaymentRequest, BookingDetailResponse>();
		   cfg.CreateMap<BookingDetailResponse, ElectronicPaymentDetails>();
		   cfg.CreateMap<ParkingLocationRate, ParkingLocationRateRequest>();
	   });
			_mapper = config.CreateMapper();

		}

		public long AddCustomerInfo(CustomerInfo model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddCustomerInfo");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.Id);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@NotifyMeBeforeBooking", model.NotifyMeBeforeBooking);
				//objCmd.Parameters.AddWithValue("@LastActiveDateTime", model.LastActiveDatetime);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				if (dtCustomer.Rows.Count == 0) throw new AppException("Could not add info");
				else return Convert.ToInt64(dtCustomer.Rows[0]["CustomerId"]);
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
		public long AddCustomerVehicles(CustomerVehicles model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddCustomerVehicles");
			try
			{

				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.Id);
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				objCmd.Parameters.AddWithValue("@NumberPlate", model.NumberPlate);
				objCmd.Parameters.AddWithValue("@VehicleModal", model.VehicleModal);
				objCmd.Parameters.AddWithValue("@VehicleTypeId", model.VehicleTypeId);
				objCmd.Parameters.AddWithValue("@VehicleColorId", model.VehicleColorId);
				objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.VehicleManufacturerId);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return Convert.ToInt64(dtCustomer.Rows[0]["VehicleId"]);
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

		List<CustomerVehicleResponse> ICustomer.GetCustomerVehicles(long CustomerInfoId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerVehicles");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerInfoId", CustomerInfoId);
				DataSet ds = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;
				var customerVehicles = (from DataRow dr in ds.Tables[0].Rows
										select new CustomerVehicleResponse
										{
											Id = Convert.ToInt32(dr["Id"]),
											CustomerInfoId = Convert.ToInt32(dr["CustomerInfoId"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
											VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
											VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
											VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
											VehicleColorId = dr["VehicleColorId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleColorId"]),
											VehicleManufacturerId = dr["VehicleManufacturerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["VehicleManufacturerId"]),
											VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"]),
											StateCode = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).StateCode,
											VehicleCountry = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).Name,
											CountryCode = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).CountryCode,
											CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
											UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
										}).ToList();

				return customerVehicles;

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



		public CustomerVehicleResponse GetVehicleInfoById(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetVehicleInfoById");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", Id);

				DataSet ds = objSQL.FetchDB(objCmd);
				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();
				StatesMst state = null; Countries country = null;

				var customerVehicles = (from DataRow dr in ds.Tables[0].Rows
										select new CustomerVehicleResponse
										{
											Id = Convert.ToInt32(dr["Id"]),
											CustomerInfoId = Convert.ToInt32(dr["CustomerInfoId"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
											VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
											VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
											VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
											VehicleColorId = dr["VehicleColorId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleColorId"]),
											VehicleManufacturerId = dr["VehicleManufacturerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["VehicleManufacturerId"]),
											VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"]),
											StateCode = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).StateCode,
											VehicleCountry = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).Name,
											CountryCode = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).CountryCode,
											CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
											UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
										}).FirstOrDefault();


				return customerVehicles;
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

		public long UpdateNotificationMode(UpdateNotificationRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateNotificationMode");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				objCmd.Parameters.AddWithValue("@ReservationNotificationMode", model.ReservationNotificationMode);
				objCmd.Parameters.AddWithValue("@PaymentNotificationMode", model.PaymentNotificationMode);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);


				if (dtCustomer.Rows.Count == 0) throw new AppException("Could not update notification");
				return Convert.ToInt64(dtCustomer.Rows[0]["CustomerId"]);
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

		public long AddNotification(Notification model)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddNotification");
			try
			{

				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@NotificationType", model.NotificationType);
				objCmd.Parameters.AddWithValue("@NotificationDateTime", model.NotificationDateTime);
				objCmd.Parameters.AddWithValue("@Message", model.Message);
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				return Convert.ToInt64(dtCustomer.Rows[0]["NotificationId"]);
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

		public List<NotificationResponse> GetNotifications(long CustomerInfoId, long ParkingLocationId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetNotifications");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerInfoId", CustomerInfoId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);

				DataTable dtNotification = objSQL.FetchDT(objCmd);
				var notification = (from DataRow dr in dtNotification.Rows
									select new NotificationResponse
									{

										CustomerId = Convert.ToInt64(dr["CustomerId"]),
										ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										NotificationType = Convert.ToString(dr["NotificationType"]),
										NotificationDateTime = Convert.ToDateTime(dr["NotificationDateTime"]),
										Message = Convert.ToString(dr["Message"]),
										CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),

									}).ToList();
				return notification;
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
		public (long, string) AddGuestUser(GuestUserRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddGuestUser");
			try
			{
				Random r = new Random();
				string OTP = r.Next(1000, 9999).ToString();
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@OTP", OTP);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				if (dtCustomer.Rows.Count > 0)
				{
					string MsgText = $"Your Flix Valet App otp is: {OTP}";

					_smsService.SendSMS(MsgText, model.Mobile);
					DataRow dataRow = dtCustomer.Rows[0];

					return (Convert.ToInt64(dataRow["CustomerId"]), Convert.ToString(dataRow["Msg"]));
				}
				throw new AppException("Guest couldn't be added.");
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

		public (long, string) ResendOTP(CustomerIdModel model)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_ResendOTP");
			try
			{
				Random r = new Random();
				string OTP = r.Next(1000, 9999).ToString();
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				objCmd.Parameters.AddWithValue("@OTP", OTP);

				DataSet ds = objSQL.FetchDB(objCmd);

				var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var custInfo = (from DataRow dr in ds.Tables[1].Rows
								select new
								{
									CustomerId = Convert.ToInt64(dr["Id"]),
									Mobile = Convert.ToString(dr["Mobile"])
								}).FirstOrDefault();

				if (custInfo != null)
				{
					string MsgText = $"Your Flix Valet App otp is: {OTP}";
					_smsService.SendSMS(MsgText, custInfo.Mobile);
				}

				return (custInfo is null ? 0 : custInfo.CustomerId, OTP);
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

		public long VerifyOTP(VerifyOTPRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_VerifyOTP");

			try
			{
				objCmd.Parameters.AddWithValue("@OTP", model.OTP);
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				if (dtCustomer.Rows.Count == 0) throw new AppException("Verification failed");

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

		public UpcomingBookingResponse GetUpcomingBookings(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetUpcomingBookings");
			try
			{
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@LocationsList", LocationsList);
				objCmd.Parameters.AddWithValue("@SearchDate", SearchDate);

				DataTable dtbooking = objSQL.FetchDT(objCmd);
				if (dtbooking.Rows.Count > 0)
				{
					var lstBooking = (from DataRow dr in dtbooking.Rows
									  select new UpcomingBookingList
									  {
										  Id = Convert.ToInt64(dr["Id"]),
										  CustomerName = Convert.ToString(dr["CustomerName"]),
										  Address = Convert.ToString(dr["Address"]),
										  ProfilePic = Convert.ToString(dr["ProfilePic"]),
										  Mobile = Convert.ToString(dr["Mobile"]),
										  Email = Convert.ToString(dr["Email"]),
										  NetAmount = Convert.ToDecimal(dr["NetAmount"]),
										  ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
										  BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
										  StartDate = Convert.ToDateTime(dr["StartDate"]),
										  EndDate = Convert.ToDateTime(dr["EndDate"]),
										  StartTime = Convert.ToString(dr["StartTime"]),
										  EndTime = Convert.ToString(dr["EndTime"]),
										  Duration = Convert.ToDecimal(dr["Duration"]),
										  NumberPlate = Convert.ToString(dr["NumberPlate"]),
										  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
										  BookingStartDateTime = dr["BookingStartDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingStartDatetime"]) : (DateTime?)null,
										  BookingEndDateTime = dr["BookingEndDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingEndDatetime"]) : (DateTime?)null
                                      }).ToList();

					return new UpcomingBookingResponse { UpcomingBooking = lstBooking, Total = Convert.ToInt32(dtbooking.Rows[0]["TotalCount"]) };
				}
				return new UpcomingBookingResponse { UpcomingBooking = new List<UpcomingBookingList>(), Total = 0 };
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

		public BookingDetailResponse GetBookingDetails(long Id, DateTime CurrentDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDetails_v8");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", Id);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

				DataSet dsBooking = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null;
				var bookingDetails = (from DataRow dr in dsBooking.Tables[0].Rows
									  select new BookingDetailResponse
									  {
										  Id = Convert.ToInt64(dr["Id"]),
										  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										  Name = Convert.ToString(dr["CustomerName"]),
										  Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
										  MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
										  Email = Convert.ToString(dr["Email"]),
										  Address = Convert.ToString(dr["Address"]),
										  ProfilePic = Convert.ToString(dr["ProfilePic"]),
										  Charges = Convert.ToDecimal(dr["Charges"]),
										  TotalCharges = Convert.ToDecimal(dr["NetAmount"]),
										  StartDate = Convert.ToDateTime(dr["StartDate"]),
										  EndDate = Convert.ToDateTime(dr["EndDate"]),
										  StartTime = Convert.ToString(dr["StartTime"]),
										  EndTime = Convert.ToString(dr["EndTime"]),
										  Duration = Convert.ToDecimal(dr["Duration"]),
										  NumberPlate = Convert.ToString(dr["NumberPlate"]),
										  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
										  VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
										  VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
										  VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
										  CustomerInfoId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
										  CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
										  EnterExitId = dr["EnterExitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["EnterExitId"]),
										  Notes = Convert.ToString(dr["Notes"]),
										  EntryDate = Convert.ToString(dr["EntryDate"]),
										  EnterTime = Convert.ToString(dr["EnterTime"]),
										  ExitDate = Convert.ToString(dr["ExitDate"]),
										  ExitTime = Convert.ToString(dr["ExitTime"]),
										  Symbol = Convert.ToString(dr["Symbol"]),
                                          Currency = Convert.ToString(dr["Currency"]),
                                          IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
										  TimeZone = Convert.ToString(dr["TimeZone"]),
										  TaxAmount = dr["Tax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Tax"]),
										  MaxDurationofSlab = dr["MaxDurationofSlab"] == DBNull.Value ? (double?)null : Convert.ToDouble(dr["MaxDurationofSlab"]),
										  MaxRateofSlab = dr["MaxRateofSlab"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["MaxRateofSlab"]),
										  MonthlyPassMessage = Convert.ToInt32(dr["bookingtypeid"]).Equals(2) ? "(Monthly Pass)" : null,
										  HasPaymentSetup = Convert.ToBoolean(dr["HasPaymentSetup"]),
										  TaxPercent = Convert.ToDecimal(dr["TaxPercent"]),
										  BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
										  OverweightCharges = Convert.ToDecimal(dr["ExtraCharges"]),
										  LocationName = Convert.ToString(dr["LocationName"]),
										  LocationAddress = Convert.ToString(dr["LocationAddress"]),
										  BookingTypeId = Convert.ToInt32(dr["BookingTypeId"]),
										  ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
										  BookingCategoryId = dr["BookingCategoryId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["BookingCategoryId"]),
										  BookingNotes = Convert.ToString(dr["BookingNotes"]),
										  PaymentMethod = Convert.ToString(dr["PaymentMethod"]),
										  LocationId = Convert.ToString(dr["LocationId"]),
										  AccessToken = Convert.ToString(dr["AccessToken"]),
                                          ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
                                          SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
                                          IsProduction = dr["IsProduction"] == DBNull.Value ? false : Convert.ToBoolean(dr["IsProduction"]),
                                          ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
                                          LocationConvenienceFee = Convert.ToDecimal(dr["LocationConvenienceFee"]),
                                           StartBookingDate= dr["BookingStartDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingStartDatetime"]) : (DateTime?)null,
                                          LastBookingDate = dr["BookingEndDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingEndDatetime"]) : (DateTime?)null
                                      }).FirstOrDefault();

				var extraDetails = (from DataRow dr in dsBooking.Tables[1].Rows
									select new
									{
										PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
										PaymentMode = Convert.ToString(dr["PaymentMode"])
                                    }).FirstOrDefault();
				var rates = (from DataRow dr in dsBooking.Tables[2].Rows
							 select new ParkingLocationRate
							 {
								 BookingType = ((EBookingType)(dr["BookingTypeId"])).ToString(),
								 Duration = ((EBookingType)(dr["BookingTypeId"])) == EBookingType.Monthly ?  Convert.ToInt32(dr["DurationUpto"]) * 30 : Convert.ToInt32(dr["DurationUpto"]),
								 Charges = Convert.ToDecimal(dr["Rate"])
							 }).ToList();

				rates = rates.Where(a => a.BookingType.Equals(((EBookingType)bookingDetails.BookingTypeId).ToString())).ToList();

				var ratelist = _mapper.Map<List<ParkingLocationRateRequest>>(rates);
				state = null; Countries country = null;
				var OwnerInfo = (from DataRow dr in dsBooking.Tables[3].Rows
								 select new ParkingOwnerInfo
								 {
									 ParkingBusinessOwnerId = Convert.ToInt64(dr["Id"]),
									 BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
									 Address = Convert.ToString(dr["Address"]),
									 City = Convert.ToString(dr["City"]),
									 ZipCode = Convert.ToString(dr["ZipCode"]),
									 State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
									 StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
									 Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
									 CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
									 LogoUrl = Convert.ToString(dr["LogoUrl"]),
								 }).FirstOrDefault();


				if (bookingDetails != null)
				{
					bookingDetails.Duration = bookingDetails.Duration;
                    bookingDetails.BookingType = ((EBookingType)bookingDetails.BookingTypeId).ToString();
					bookingDetails.PaidAmount = extraDetails.PaidAmount;
					bookingDetails.IsGuest = bookingDetails.Name.ToLower() == "guest user" ? true : false;
					var minutes = (int)((bookingDetails.Duration - Math.Truncate(bookingDetails.Duration)) * 60);
					var duration = ((int)bookingDetails.Duration == 0 ? "" : (int)bookingDetails.Duration + " hours") + (minutes == 0 ? "" : " " + minutes + " minutes");
					bookingDetails.MaxStay = duration.Trim();
					bookingDetails.PaymentMode = extraDetails.PaymentMode;

					bookingDetails = CalculateExtraHourCharges(bookingDetails, CurrentDate, ratelist);
					if (bookingDetails.TotalOverStayDuration > 0.00)
					{
						minutes = (int)((bookingDetails.TotalOverStayDuration - Math.Truncate(Convert.ToDouble(bookingDetails.TotalOverStayDuration))) * 60);
						duration = ((int)bookingDetails.TotalOverStayDuration == 0 ? "" : (int)bookingDetails.TotalOverStayDuration + " hours") + (minutes == 0 ? "" : " " + minutes + " minutes");
						bookingDetails.OverStayHours = bookingDetails.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? "0" : duration.Trim();
						if ((int)bookingDetails.TotalOverStayDuration == 0 && minutes == 0 && bookingDetails.PaidAmount == bookingDetails.PreviousBookingAmount)
							bookingDetails.IsAdditionalChargesApplied = false;
					}

					bookingDetails.ParkingOwnerInfo = OwnerInfo;
				}
				return bookingDetails;
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

		public BookingDetailResponse CalculateExtraHourCharges(BookingDetailResponse bookingDetail, DateTime CurrentDate, List<ParkingLocationRateRequest> rates)
		{
			bookingDetail.PreviousBookingAmount = bookingDetail.TotalCharges;
			if (bookingDetail.EnterExitId != null)
			{

				var (bookingList, OverStayResponse) = parkingHelper.AdditionalPaymentLogic(bookingDetail, CurrentDate, bookingDetail.IsEarlyBirdOfferApplied, rates);

				bookingDetail.TotalOverStayDuration = bookingDetail.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? 0 : Convert.ToDouble(OverStayResponse.OverStayDuration);
				bookingDetail.TotalOverStayCharges = bookingDetail.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? 0 : parkingHelper.RoundOff(bookingList.Sum(a => a.Charges));

				bookingDetail.UnpaidAmount = bookingDetail.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? 0 : parkingHelper.RoundOff(OverStayResponse.NetAmount - bookingDetail.PaidAmount);

				bookingDetail.UnpaidAmountWithConvenienceFee = bookingDetail.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? 0 : parkingHelper.RoundOff(OverStayResponse.NetAmountWithConvenienceFee - bookingDetail.PaidAmount);
				bookingDetail.TotalCharges = bookingDetail.BookingCategoryId == ((int)EBookingCategories.NoCharge) ? 0 : OverStayResponse.NetAmount;

				// bookingDetail.UnpaidAmount = (bookingDetail.BookingType.ToLower().Equals("monthly")) ? Convert.ToDecimal(bookingDetail.TotalOverStayCharges) : parkingHelper.RoundOff((bookingDetail.TotalCharges - bookingDetail.PaidAmount) + Convert.ToDecimal(bookingDetail.TotalOverStayCharges));
				bookingDetail.IsAdditionalChargesApplied = bookingDetail.UnpaidAmount > 0 ? true : false;
				bookingDetail.BookingAmount = OverStayResponse.NewBookingAmount;

				bookingDetail.TaxAmountWithConvenienceFee = OverStayResponse.TaxAmountWithConvenienceFee;
				bookingDetail.FinalAmountWithConvenienceFee = OverStayResponse.NetAmountWithConvenienceFee;
			}

			bookingDetail.FinalAmount = bookingDetail.BookingCategoryId == 3 ? 0 : bookingDetail.TotalCharges;
			return bookingDetail;

		}


		public List<VehicleBookingResponse> GetBookingIdByVehicleNumber(string ParkingLocationId, DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingIdByVehicle");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				objCmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);
				if (!string.IsNullOrEmpty(CustomerName))
					objCmd.Parameters.AddWithValue("@CustomerName", CustomerName);
				objCmd.Parameters.AddWithValue("@IsExit", IsExit);

				DataTable dtBooking = objSQL.FetchDT(objCmd);

				var bookingResponse = (from DataRow dr in dtBooking.Rows
									   select new VehicleBookingResponse
									   {
										   BookingId = Convert.ToInt64(dr["Id"]),
										   EntryDate = Convert.ToString(dr["EntryDate"]),
										   ExitDate = Convert.ToString(dr["ExitDate"]),
										   EnterTime = Convert.ToString(dr["EnterTime"]),
										   ExitTime = Convert.ToString(dr["ExitTime"]),
										   BookingTypeId = Convert.ToInt32(dr["BookingTypeId"])
									   }).ToList();
				return bookingResponse;
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

		public Tuple<List<VehicleBookingResponse>, WhiteListCustomers, BusinessOfficeEmployeeDetails> GetBookingIdByVehicleNumberV1(string ParkingLocationId, DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingIdByVehicle_v3");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				objCmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);
				if (!string.IsNullOrEmpty(CustomerName))
					objCmd.Parameters.AddWithValue("@CustomerName", CustomerName);
				objCmd.Parameters.AddWithValue("@IsExit", IsExit);


				DataSet dsBooking = objSQL.FetchDB(objCmd);
				DataTable dtBooking = dsBooking.Tables[0];
				DataTable dtWhiteList = dsBooking.Tables[1];
				DataTable dtChargeBack = dsBooking.Tables[2];

				var bookingResponse = (from DataRow dr in dtBooking.Rows
									   select new VehicleBookingResponse
									   {
										   BookingId = Convert.ToInt64(dr["Id"]),
										   EntryDate = Convert.ToString(dr["EntryDate"]),
										   ExitDate = Convert.ToString(dr["ExitDate"]),
										   EnterTime = Convert.ToString(dr["EnterTime"]),
										   ExitTime = Convert.ToString(dr["ExitTime"]),
										   BookingTypeId = Convert.ToInt32(dr["BookingTypeId"]),
                                           BookingStartDateTime = dr["BookingStartDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingStartDatetime"]) : (DateTime?)null,
                                           BookingEndDateTime = dr["BookingEndDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingEndDatetime"]) : (DateTime?)null
                                       }).ToList();

				var whiteList = (from DataRow dr in dtWhiteList.Rows
								 select new WhiteListCustomers
								 {
									 WhiteListCustomerId = Convert.ToInt32(dr["Id"]),
									 NumberPlate = Convert.ToString(dr["NumberPlate"]),
									 VehicleModal = Convert.ToString(dr["VehicleModal"])
								 }).FirstOrDefault();

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();
				StatesMst state = null; Countries country = null;

				var chargeBack = (from DataRow dr in dtChargeBack.Rows
								  select new BusinessOfficeEmployeeDetails
								  {
									  BusinessOfficeEmployeeId = Convert.ToInt32(dr["Id"]),
									  OfficeDuration = Convert.ToInt32(dr["OfficeDuration"]),
									  BusinessOfficeId = Convert.ToInt32(dr["BusinessOfficeId"]),
									  CustomerVehicleId = Convert.ToInt32(dr["CustomerVehicleId"]),
									  CustomerInfoId = Convert.ToInt32(dr["CustomerInfoId"]),
									  NumberPlate = Convert.ToString(dr["NumberPlate"]),
									  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
									  VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
									  VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
									  VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
									  VehicleColorId = dr["VehicleColorId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleColorId"]),
									  VehicleManufacturerId = dr["VehicleManufacturerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["VehicleManufacturerId"]),
									  VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"]),
									  StateCode = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).StateCode,
									  VehicleCountry = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).Name,
									  CountryCode = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).CountryCode,
								  }).FirstOrDefault();
				return Tuple.Create(bookingResponse, whiteList, chargeBack);
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

		public EnterRequestDetails CustomerEnterToLocation(CustomerEnterRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CustomerEnterToLocation");
			try
			{
				objCmd.Parameters.AddWithValue("@Id", model.Id);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@EntryDate", model.EntryDate);
				objCmd.Parameters.AddWithValue("@EnterTime", model.EnterTime);
				objCmd.Parameters.AddWithValue("@Notes", model.Notes);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);


				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var customer = (from DataRow dr in dtCustomer.Rows
								select new EnterRequestDetails
								{
									EnterId = Convert.ToInt64(dr["Id"]),
									Amount = Convert.ToDecimal(dr["Amount"]),
									BookingTypeId = Convert.ToInt32(dr["BookingTypeId"]),
									Mobile = Convert.ToString(dr["Mobile"]),
									Currency = Convert.ToString(dr["Currency"]),
									SendeTicket = Convert.ToBoolean(dr["SendeTicket"])
								}).FirstOrDefault();

				return customer;
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

		public long CustomerExitFromLocation(CustomerExitRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CustomerExitFromLocation_v3");
			try
			{
				DateTime DtExitDate = Convert.ToDateTime(model.ExitDate.ToShortDateString());

				objCmd.Parameters.AddWithValue("@EnterExitId", model.EnterExitId);
				objCmd.Parameters.AddWithValue("@ExitDate", DtExitDate);
				objCmd.Parameters.AddWithValue("@ExitTime", model.ExitTime);
				objCmd.Parameters.AddWithValue("@Notes", model.Notes);

				if (model.BookingCategoryId == (int)EBookingCategories.ChargeBack)
				{
					AdditionalPaymentRequest payment = new AdditionalPaymentRequest();
					payment.BookingId = model.BookingId;
					payment.UserId = model.UserId;
					payment.ExitDate = model.ExitDate;
					payment.StartDate = model.StartDate;
					payment.EndDate = model.EndDate;
					payment.StartTime = model.StartTime;
					payment.EndTime = model.EndTime;
					payment.EntryDate = model.EntryDate;
					payment.EnterTime = model.EnterTime;
					payment.TotalOverStayDuration = model.TotalOverStayDuration;
					payment.TotalOverStayCharges = model.TotalOverStayCharges;
					payment.UnpaidAmount = model.UnpaidAmount;
					payment.PaymentMode = "Chargeback";
					payment.Currency = "USD";
					payment.MaxDurationofSlab = model.MaxDurationofSlab;
					payment.MaxRateofSlab = model.MaxRateofSlab;
					payment.Mobile = model.Mobile;
					MakeAdditionalPayment(payment, "");
				}

				DataTable dtCustomer = objSQL.FetchDT(objCmd);
				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

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

		public CustomerIdModel GetCustomerIdByVehicleNumber(string VehicleNumber)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerByVehicleNumber");
			try
			{
				objCmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);

				DataSet ds = objSQL.FetchDB(objCmd);

				var customer = (from DataRow dr in ds.Tables[0].Rows
								select new CustomerIdModel
								{
									CustomerInfoId = Convert.ToInt64(dr["CustomerInfoId"]),
								}).FirstOrDefault();
				return customer;
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

		public ParkedVehiclesResponse GetParkedVehicles(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkedVehicles");
			try
			{
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@LocationsList", LocationsList);
				objCmd.Parameters.AddWithValue("@SearchDate", SearchDate);

				DataTable dtbooking = objSQL.FetchDT(objCmd);
				if (dtbooking.Rows.Count > 0)
				{
					var listVehicles = (from DataRow dr in dtbooking.Rows
										select new ParkedVehiclesList
										{
											Id = Convert.ToInt64(dr["Id"]),
											CustomerId = Convert.ToInt64(dr["CustomerId"]),
											CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
											Address = Convert.ToString(dr["Address"]),
											CustomerName = Convert.ToString(dr["CustomerName"]),
											ProfilePic = Convert.ToString(dr["ProfilePic"]),
											Mobile = Convert.ToString(dr["Mobile"]),
											Email = Convert.ToString(dr["Email"]),
											NetAmount = Convert.ToDecimal(dr["NetAmount"]),
											ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
											BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
											StartDate = Convert.ToDateTime(dr["StartDate"]),
											EndDate = Convert.ToDateTime(dr["EndDate"]),
											StartTime = Convert.ToString(dr["StartTime"]),
											EndTime = Convert.ToString(dr["EndTime"]),
											Duration = Convert.ToDecimal(dr["Duration"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
											DamageVehicleId = dr["DamageVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["DamageVehicleId"]),
											IsDamageReported = Convert.ToBoolean(dr["IsDamageReported"]),
											BookingStartDateTime = dr["BookingStartDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingStartDatetime"]) : (DateTime?)null,
											BookingEndDateTime = dr["BookingEndDatetime"] != System.DBNull.Value ? Convert.ToDateTime(dr["BookingEndDatetime"]) : (DateTime?)null
                                        }).ToList();

					return new ParkedVehiclesResponse { ParkedVehicles = listVehicles, Total = Convert.ToInt32(dtbooking.Rows[0]["TotalCount"]) };
				}
				return new ParkedVehiclesResponse { ParkedVehicles = new List<ParkedVehiclesList>(), Total = 0 };
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

		public long AddDamageVehicle(VehicleDamage model)
		{


			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddDamageVehicle");
			try
			{
				var Images = String.Join(",", model.Images.Select(x => x.ImageURL));
				objCmd.Parameters.AddWithValue("@DamageVehicleId", model.Id);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@Notes", model.Notes);
				objCmd.Parameters.AddWithValue("@FilePath", Images);
				DataSet ds = objSQL.FetchDB(objCmd);

				var Error = Convert.ToString(ds.Tables[1].Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				if (ds.Tables[0].Rows.Count == 0) throw new AppException("Damage could not be reported");
				return Convert.ToInt64(ds.Tables[0].Rows[0]["Id"]);
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

		public long AddCustomerAddress(CustomerAddress model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddCustomerAddress");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@Address", model.Address);
				objCmd.Parameters.AddWithValue("@City", model.City);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@ZipCode", model.ZipCode);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);
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

		public CustomerAddress GetCustomerAddressById(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerAddressById");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", Id);

				DataSet ds = objSQL.FetchDB(objCmd);

				if (ds.Tables[0].Rows.Count > 0)
				{
					var customer = (from DataRow dr in ds.Tables[0].Rows
									select new CustomerAddress
									{
										CustomerId = Convert.ToInt64(dr["Id"]),
										Address = Convert.ToString(dr["Address"]),
										City = Convert.ToString(dr["City"]),
										State = Convert.ToString(dr["State"]),
										StateCode = Convert.ToString(dr["StateCode"]),
										Country = Convert.ToString(dr["Country"]),
										CountryCode = Convert.ToString(dr["SortName"]),
										ZipCode = Convert.ToString(dr["ZipCode"]),
										CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
										UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"]),

									}).FirstOrDefault();
					return customer;
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

		public string CancelBooking(CancelBookingModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CheckBookingCancellationv1");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
				DataTable dtBooking = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtBooking.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var booking = (from DataRow dr in dtBooking.Rows
							   select new CancelBookingDetails
							   {
								   PaymentProvider = Convert.ToString(dr["PaymentProvider"]),
								   Amount = Convert.ToDecimal(dr["Amount"]),
								   Currency = Convert.ToString(dr["Currency"]),
								   TransactionId = Convert.ToString(dr["TransactionId"]),
								   SecretKey = Convert.ToString(dr["SecretKey"]),
								   AccessToken = Convert.ToString(dr["AccessToken"]),
								   IsProduction = Convert.ToBoolean(dr["IsProduction"]),
								   FirstName = Convert.ToString(dr["FirstName"]),
								   LastName = Convert.ToString(dr["LastName"]),
								   Email = Convert.ToString(dr["Email"]),
								   Mobile = Convert.ToString(dr["Mobile"]),
								   LocationId = Convert.ToString(dr["LocationId"]),
								   ApiKey = Convert.ToString(dr["ApiKey"])
							   }).FirstOrDefault();
				if (booking != null)
				{
					var (RefundId, RefundMessage) = RefundPayment(booking);


					if (!string.IsNullOrEmpty(RefundId))
					{
						objCmd = new SqlCommand("sp_CancelBooking");
						objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
						objCmd.Parameters.AddWithValue("@RefundId", RefundId);
						objCmd.Parameters.AddWithValue("@RefundStatus", RefundMessage);
						objSQL.UpdateDB(objCmd, true);
					}
					return RefundMessage;
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


		public (string, string) RefundPayment(CancelBookingDetails details)
		{
			string RefundId = string.Empty, RefundMessage = string.Empty; object RefundResponse;
			if (details.PaymentProvider.Equals(EPaymentMode.Square.ToString()))
			{
				RefundResponse = _squareRepo.RefundPayment(details);

				if (RefundResponse is PaymentRefund)
				{
					PaymentRefund refundObj = (PaymentRefund)RefundResponse;
					RefundMessage = refundObj.Status;
					if (refundObj.Status.Equals(ESquareRefundStatus.COMPLETED.ToString())
					 || refundObj.Status.Equals(ESquareRefundStatus.PENDING.ToString()))
						RefundId = refundObj.Id;

					else
					{
						sendCancelBookingFailureEmail(details);
						throw new AppException(refundObj.Status);
					}
				}

				else
				{
					sendCancelBookingFailureEmail(details);
					throw new AppException(RefundResponse.ToString());
				}

			}
			if (details.PaymentProvider.Equals(EPaymentMode.Stripe.ToString()))
			{
				RefundResponse = _stripeRepo.RefundPayment(details);
				if (RefundResponse is Stripe.Refund)
				{
					Stripe.Refund refundObj = (Stripe.Refund)RefundResponse;
					RefundMessage = refundObj.Status;
					if (refundObj.Status.Equals("succeeded"))
						RefundId = refundObj.Id;

					else
					{
						sendCancelBookingFailureEmail(details);
						throw new AppException(refundObj.Status);
					}
				}

				else
				{
					sendCancelBookingFailureEmail(details);
					throw new AppException(RefundResponse.ToString());
				}
			}
			if (details.PaymentProvider.Equals(EPaymentMode.Paypal.ToString()))
			{
				RefundResponse = _paypalRepo.RefundPayment(details);
				if (RefundResponse is PaypalRefundResponse)
				{
					PaypalRefundResponse refundObj = (PaypalRefundResponse)RefundResponse;
					RefundMessage = refundObj.Target.ProcessorResponseText;
					RefundId = refundObj.Target.RefundedTransactionId;
				}
				else
				{
					var CancelResponse = _paypalRepo.CancelPayment(details);
					if (CancelResponse is PaypalPaymentApiResponse)
					{
						PaypalPaymentApiResponse cancelObj = (PaypalPaymentApiResponse)CancelResponse;
						RefundMessage = cancelObj.Target.ProcessorResponseText;
						RefundId = cancelObj.Target.Id;
					}
					else
					{
						PaypalErrorResponse cancel = (PaypalErrorResponse)CancelResponse;
						sendCancelBookingFailureEmail(details);
						throw new AppException(cancel.Message);
					}
				}
			}

			return (RefundId, RefundMessage);
		}

		private void sendCancelBookingFailureEmail(CancelBookingDetails details)
		{
			string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\CancelBookingFailureEmail.html");
			MailText = string.Format(MailText, details.FirstName.Trim(), details.LastName.Trim(), _appSettings.AppName, details.Email);

			_emailService.Send(
				to: details.Email,
				subject: $"Refund Cancelled {_appSettings.AppName}",
				html: MailText
			);

		}


		private string getEmailTemplateText(string fileRelativePath)
		{
			string physicalPath = Directory.GetCurrentDirectory();
			string FilePath = physicalPath + fileRelativePath;
			FilePath = Path.GetFullPath(FilePath);
			StreamReader str = new StreamReader(FilePath);
			string MailText = str.ReadToEnd();
			str.Close();
			return MailText;
		}

		public BookingDetailsByIdResponse GetBookingDetailsById(long Id)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDetailsById");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", Id);

				DataSet ds = objSQL.FetchDB(objCmd);


				var booking = (from DataRow dr in ds.Tables[0].Rows
							   select new BookingDetailsByIdResponse
							   {
								   Id = Convert.ToInt32(dr["Id"]),
								   StartDate = Convert.ToDateTime(dr["StartDate"]),
								   EndDate = Convert.ToDateTime(dr["EndDate"]),
								   StartTime = Convert.ToString(dr["StartTime"]),
								   EndTime = Convert.ToString(dr["EndTime"]),
								   BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
								   ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
								   TotalCharges = Convert.ToDecimal(dr["TotalCharges"]),
								   NumberPlate = Convert.ToString(dr["NumberPlate"])
							   }).FirstOrDefault();


				return booking;
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


		public long UpdateGuestUserDetails(GuestUserDetailRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateGuestUserDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@GuestUserId", model.Id);
				objCmd.Parameters.AddWithValue("@VehicleName", model.VehicleName);
				objCmd.Parameters.AddWithValue("@VehicleModal", model.VehicleModal);
				objCmd.Parameters.AddWithValue("@VehicleNumber", model.VehicleNumber);
				objCmd.Parameters.AddWithValue("@VehicleColorId", model.VehicleColorId);
				objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.VehicleManufacturerId);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@VehicleTypeId", model.VehicleTypeId);
				objCmd.Parameters.AddWithValue("@UpdatedDate", model.UpdatedDate);
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
		public CustomerBookingListResponse GetCustomerBookingList(string sortColumn, string sortOrder, int pageNo, int? pageSize, long CustomerId, string BookingType, DateTime SearchDate, string SearchMode)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerBookingList");
			try
			{
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				objCmd.Parameters.AddWithValue("@BookingType", BookingType);
				if (!string.IsNullOrEmpty(SearchMode))
					objCmd.Parameters.AddWithValue("@SearchMode", SearchMode);
				objCmd.Parameters.AddWithValue("@SearchDate", SearchDate);

				DataTable dtbooking = objSQL.FetchDT(objCmd);
				if (dtbooking.Rows.Count > 0)
				{
					var listBookings = (from DataRow dr in dtbooking.Rows
										select new CustomerBookingList
										{
											Id = Convert.ToInt64(dr["Id"]),
											Mobile = Convert.ToString(dr["Mobile"]),
											StartDate = Convert.ToDateTime(dr["StartDate"]),
											EndDate = Convert.ToDateTime(dr["EndDate"]),
											StartTime = Convert.ToString(dr["StartTime"]),
											EndTime = Convert.ToString(dr["EndTime"]),
											Duration = Convert.ToDecimal(dr["Duration"]),
											LocationName = Convert.ToString(dr["LocationName"]),
											LocationPic = Convert.ToString(dr["LocationPic"]),
											Address = Convert.ToString(dr["Address"]),
											Latitude = Convert.ToString(dr["Latitude"]),
											Longitude = Convert.ToString(dr["Longitude"]),
											QRCodePath = Convert.ToString(dr["QRCodePath"])
										}).ToList();

					return new CustomerBookingListResponse
					{
						CustomerBookingList = listBookings,
						Total = Convert.ToInt32(dtbooking.Rows[0]["TotalCount"])
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

		public bool MakeAdditionalPayment(AdditionalPaymentRequest model, string paymenturl)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDataByBookingId_v1");
			try
			{

				// SqlCommand objCmd1 = new SqlCommand("sp_UpdateConvenienceFee_v2");
				// objCmd1.Parameters.AddWithValue("@BookingId", model.BookingId);
				// objCmd1.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
				// objSQL.UpdateDB(objCmd1, true);


				DateTime DtExitDate = Convert.ToDateTime(model.ExitDate.ToShortDateString());
				objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.ExitDate);

				DataSet ds = objSQL.FetchDB(objCmd);
				var BookingData = (from DataRow dr in ds.Tables[0].Rows
								   select new
								   {
									   TimeZone = Convert.ToString(dr["TimeZone"]),
									   Mobile = Convert.ToString(dr["Mobile"]),
									   Duration = Convert.ToDecimal(dr["Duration"]),
									   Charges = Convert.ToDecimal(dr["Charges"]),
									   BookingType = Convert.ToString(dr["BookingType"]),
									   LastBookingDate = Convert.ToDateTime(dr["LastBookingDate"]),
									   IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
									   TaxPercent = Convert.ToDecimal(dr["TaxPercent"]),
									   BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
									   ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
									   PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
									   PaymentMode = Convert.ToString(dr["PaymentMode"]),
									   ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
									   BookingCategoryId = Convert.ToInt32(dr["BookingCategoryId"])
								   }).FirstOrDefault();

				if (BookingData != null)
				{

					var rates = (from DataRow dr in ds.Tables[1].Rows
								 select new ParkingLocationRateRequest
								 {
									 Duration = Convert.ToInt32(dr["DurationUpto"]),
									 Charges = Convert.ToDecimal(dr["Rate"])
								 }).ToList();

					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(BookingData.TimeZone);

					BookingDetailResponse bookingDetail = _mapper.Map<BookingDetailResponse>(model);
					bookingDetail.ExitDate = DateTime.Parse(bookingDetail.ExitDate).ToShortDateString();
					bookingDetail.ExitTime = Convert.ToString(model.ExitDate.TimeOfDay);
					bookingDetail.Duration = BookingData.Duration;
					bookingDetail.Charges = BookingData.Charges;
					bookingDetail.BookingType = BookingData.BookingType;
					bookingDetail.LastBookingDate = BookingData.LastBookingDate;
					bookingDetail.TimeZone = BookingData.TimeZone;
					bookingDetail.TaxPercent = BookingData.TaxPercent;
					bookingDetail.BookingAmount = BookingData.BookingAmount;
					bookingDetail.OverweightCharges = BookingData.ExtraCharges;
					bookingDetail.PaymentMode = model.PaymentMode;
					if (model.PaymentMode == EPaymentMode.SquareCard.ToString())
					{
						bookingDetail.ConvenienceFee = Convert.ToDecimal(model.ConvenienceFee);
					}
					else
					{
						bookingDetail.ConvenienceFee = BookingData.ConvenienceFee;
					}
					bookingDetail.BookingCategoryId = BookingData.BookingCategoryId;

					var (lstBookings, OverStayResponse) = parkingHelper.AdditionalPaymentLogic(bookingDetail, model.ExitDate, BookingData.IsEarlyBirdOfferApplied, rates);
					bookingDetail.TotalOverStayCharges = parkingHelper.RoundOff(lstBookings.Sum(a => a.Charges));

					bookingDetail.UnpaidAmount = parkingHelper.RoundOff(OverStayResponse.NetAmount - BookingData.PaidAmount);

					string Notes = null;

					foreach (var item in lstBookings)
					{
						Notes += (item.StartDate + item.StartTime).ToString("MMM dd h:mm tt") + " to " + (item.EndDate + item.EndTime).ToString("MMM dd h:mm tt") + ",";
					}


					Notes = string.IsNullOrEmpty(Notes) ? Notes : Notes.TrimEnd(',');
					if (lstBookings == null || lstBookings.Count == 0)
						Notes = (model.StartDate + TimeSpan.Parse(model.StartTime)).ToString("MMM dd h:mm tt") + " to " + (model.EndDate + TimeSpan.Parse(model.EndTime)).ToString("MMM dd h:mm tt");

					objCmd = new SqlCommand("sp_MakeAdditionalPayment_v2");

					objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
					objCmd.Parameters.AddWithValue("@UserId", model.UserId);
					objCmd.Parameters.AddWithValue("@ExitDate", DtExitDate);
					objCmd.Parameters.AddWithValue("@ExitTime", model.ExitDate.TimeOfDay);
					objCmd.Parameters.AddWithValue("@OverStayChargesTotal", bookingDetail.TotalOverStayCharges);
					objCmd.Parameters.AddWithValue("@TaxAmount", OverStayResponse.TaxAmount);
					objCmd.Parameters.AddWithValue("@BookingAmount", OverStayResponse.NewBookingAmount);
					objCmd.Parameters.AddWithValue("@NetAmount", OverStayResponse.NetAmount);
					objCmd.Parameters.AddWithValue("@UnpaidAmount", bookingDetail.UnpaidAmount);
					objCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
					objCmd.Parameters.AddWithValue("@Currency", model.Currency);
					objCmd.Parameters.AddWithValue("@MaxDurationofSlab", OverStayResponse.Duration);
					objCmd.Parameters.AddWithValue("@MaxRateofSlab", OverStayResponse.Charges);
					objCmd.Parameters.AddWithValue("@Notes", Notes);
					if (model.PaymentMode.ToLower() == EPaymentMode.SquareCard.ToString().ToLower())
					{
						objCmd.Parameters.AddWithValue("@TransactionId", model.TransactionId);
					}
					objCmd.Parameters.AddWithValue("@BookingDetailRef", MapDataTable.ToDataTable(lstBookings));


					DataTable dtExtended = FetchDTSpecialCase(objCmd, model.PaymentMode.ToLower().Equals("electronic"), false, paymenturl, (string.IsNullOrEmpty(model.Mobile)) ? BookingData.Mobile : model.Mobile);

					var Error = Convert.ToString(dtExtended.Rows[0]["Error"]);
					if (!string.IsNullOrEmpty(Error))
						throw new AppException(Error);
					return true;
				}
				else throw new AppException("No record found");
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

		public DataTable FetchDTSpecialCase(SqlCommand p_objSQLCmd, bool IsElectronicPay = false, bool IsFCM = false, string param1 = "", object param2 = null, object param3 = null)
		{
			string p_ConStr = ""; int g_Timeout = 60;
			try
			{
				p_ConStr = _configuration.GetConnectionString("WebApiDatabase").ToString();

				if ((p_ConStr == null ? string.Empty : p_ConStr.Trim()).Length != 0)
				{
					g_ConStr = (p_ConStr.Trim().Length == 0 ? p_ConStr : p_ConStr);
				}

				//Make Sure Connection Info ends properly.
				if (g_ConStr.Trim().Length > 0)
				{
					g_ConStr = (g_ConStr + ";").Replace(";;", ";");
				}


				if (g_ConStr.Length > 0 && g_ConStr.Contains("Connection Timeout") == false)
				{
					g_ConStr += "Connection Timeout=60;";
				}


				//initiate connection
				DBCon();
			}
			catch (System.Exception ex)
			{
				throw new Exception("DBM-DBMGR: " + ex.Message);
			}


			DataTable l_DT = new DataTable();
			SqlDataReader l_DR = null;
			try
			{
				if (g_ObjSQLCmd != null)
				{
					g_ObjSQLCmd.Dispose();
					g_ObjSQLCmd = null;
				}
				g_ObjSQLCmd = new SqlCommand();
				g_ObjSQLCmd = p_objSQLCmd;
				g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
				p_objSQLCmd.Connection = g_ObjCon;
				g_ObjSQLCmd.CommandTimeout = g_Timeout;
				if (g_ObjCon.State == ConnectionState.Closed)
				{
					g_ObjCon.Open();
				}
				g_ObjSQLCmd.Transaction = g_ObjCon.BeginTransaction();
				l_DR = g_ObjSQLCmd.ExecuteReader();

				l_DT.Load(l_DR);

				l_DR.Close();
				if (g_ObjSQLCmd.Transaction != null)
				{
					if (IsElectronicPay)
					{
						_smsService.SendSMS($"Make payment by clicking on the link - {param1}", param2.ToString());
					}

					if (IsFCM)
					{
						string fcmresponse = _firebaseRepo.SendVehicleRequestNotifications((PushNotificationModel)param2, param1, Convert.ToString(l_DT.Rows[0]["NotificationId"]));

						if (fcmresponse == null) throw new AppException("Notification is not enabled at this location");
						var FbResp = JsonSerializer.Deserialize<FirebaseResponse>(fcmresponse);

						if (FbResp.success == 0) throw new AppException("There was a problem sending notification");

					}
					g_ObjSQLCmd.Transaction.Commit();
				}
				g_ObjCon.Close();

			}
			catch (System.Exception ex)
			{
				if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
				{
					g_ObjSQLCmd.Transaction.Rollback();
				}
				if ((IsElectronicPay && ex is Twilio.Exceptions.ApiException) || ex is AppException)
					throw new AppException(ex.Message);
				else throw new Exception("DBM-004i: " + ex);
			}
			finally
			{
				if (l_DR != null)
					l_DR.Close();
			}
			return l_DT;
		}

		private void DBCon()
		{
			try
			{
				if (g_ConStr.Trim().Length == 0)
				{
					throw new Exception("Err-DBM: Connection string is not available.");
				}
				if (g_ObjCon == null)
				{

					g_ObjCon = new SqlConnection(g_ConStr);
				}//end of if
				if (g_ObjCon.State == System.Data.ConnectionState.Open)
				{
					WaitIfConnectionBusy();
				}
				if (g_ObjCon.State == System.Data.ConnectionState.Closed)
				{
					g_ObjCon.Open();
				}//end of if
			}
			catch (System.Exception ex)
			{
				throw new Exception("DBM-001: " + ex);
			}
			finally
			{
			}
		}

		private void WaitIfConnectionBusy()
		{
			int i = 0;
			//opening connection and wait if connection is still busy
			while (g_ObjCon.State != ConnectionState.Closed)
			{
				i += 1;
				if (i == 50000)
				{
					g_ObjCon.Dispose(); g_ObjCon = null;
				}
			}//end of while
		}
		public void EditCustomerInfo(EditCustomerInfoRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_EditCustomerInfo");
			try
			{
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
				objCmd.Parameters.AddWithValue("@LastName", model.LastName);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@Address", model.Address);
				objCmd.Parameters.AddWithValue("@City", model.City);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@ZipCode", model.ZipCode);
				objCmd.Parameters.AddWithValue("@Gender", model.Gender);
				DataTable dtInfo = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtInfo.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);
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

		public EditCustomerInfoRequest GetCustomerInfoById(long CustomerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerInfo");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				DataTable dtInfo = objSQL.FetchDT(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();
				StatesMst state = null; Countries country = null;
				var CustInfo = (from DataRow dr in dtInfo.Rows
								select new EditCustomerInfoRequest
								{
									UserId = Convert.ToInt64(dr["UserId"]),
									CustomerId = Convert.ToInt64(dr["CustomerId"]),
									FirstName = Convert.ToString(dr["FirstName"]),
									LastName = Convert.ToString(dr["LastName"]),
									Email = Convert.ToString(dr["Email"]),
									Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
									MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
									Address = Convert.ToString(dr["Address"]),
									StateCode = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
									State = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
									CountryCode = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
									Country = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
									City = Convert.ToString(dr["City"]),
									ZipCode = Convert.ToString(dr["ZipCode"]),
									ProfilePic = Convert.ToString(dr["ProfilePic"]),
									Gender = (!string.IsNullOrEmpty(dr["Gender"].ToString())) ? char.Parse(dr["Gender"].ToString()) : 'U',
									PaypalCustomerId = Convert.ToString(dr["PaypalCustomerId"])
								}).FirstOrDefault();
				if (CustInfo != null)
				{
					bool IsGenderAssigned = CustInfo.Gender == 'M' | CustInfo.Gender == 'F' | CustInfo.Gender == 'O';
					CustInfo.Gender = IsGenderAssigned ? CustInfo.Gender : 'U';
				}
				return CustInfo;
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

		public long DeleteCustomerVehicle(CustomerVehicleIdmodel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_DeleteCustomerVehicle");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.CustomerVehicleId;
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


		public BookingDetailsByBIdResponse GetBookingDetailsByBookingId(long BookingId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDetailsByBookingId_v1");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", BookingId);

				DataSet ds = objSQL.FetchDB(objCmd);

				var booking = (from DataRow dr in ds.Tables[0].Rows
							   select new BookingDetailsByBIdResponse
							   {
								   BookingId = Convert.ToInt32(dr["BookingId"]),
								   CustomerName = Convert.ToString(dr["CustomerName"]),
								   LocationName = Convert.ToString(dr["LocationName"]),
								   StartDate = Convert.ToDateTime(dr["StartDate"]),
								   EndDate = Convert.ToDateTime(dr["EndDate"]),
								   StartTime = Convert.ToString(dr["StartTime"]),
								   EndTime = Convert.ToString(dr["EndTime"]),
								   BookingType = Convert.ToString(dr["BookingType"]),
								   Amount = Convert.ToDecimal(dr["Amount"]),
								   ExtraCharges = Convert.ToDecimal(dr["ExtraCharges"]),
								   Tax = Convert.ToDecimal(dr["Tax"]),
								   TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
								   PaymentMode = Convert.ToString(dr["PaymentMode"]),
								   TransactionId = Convert.ToString(dr["TransactionId"]),
								   IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
								   IsNightFareOfferApplied = Convert.ToBoolean(dr["IsNightFareOfferApplied"]),
                                   ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
								   BookingCategoryId = Convert.ToInt32(dr["BookingCategoryId"]),
								   PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
								   IsCancelled = Convert.ToBoolean(dr["IsCancelled"]),
                                   NumberPlate = Convert.ToString(dr["NumberPlate"]),
                               }).FirstOrDefault();

				booking.UnpaidAmount = booking.TotalAmount - booking.PaidAmount;
				var notes = (from DataRow dr in ds.Tables[1].Rows
							 select new EnterExit
							 {
								 EntryDate = Convert.ToDateTime(dr["EntryDate"]),
								 Notes = Convert.ToString(dr["Notes"])
							 }).ToList();

				booking.PaymentMode = booking.PaymentMode == "Unpaid" ? booking.TotalAmount <= 0 ? "Free" : "Unpaid" : booking.PaymentMode;

				if (booking != null)
					booking.ValetNotes = notes;


				return booking;
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


		public long VerifyGuestVehicle(long CustomerId, string NumberPlate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_VerifyGuestVehicle");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				objCmd.Parameters.AddWithValue("@NumberPlate", NumberPlate);
				DataTable dtVehicleId = objSQL.FetchDT(objCmd);

				return Convert.ToInt64(dtVehicleId.Rows[0]["Id"]);
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

		public PushNotificationModel RequestVehicle(RequestVehicleModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetVehicleRequestData");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.NotificationDateTime);

				DataSet dsDetails = objSQL.FetchDB(objCmd);
				var Error = Convert.ToString(dsDetails.Tables[0].Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var VehicleRequest = (from DataRow dr in dsDetails.Tables[1].Rows
									  select new PushNotificationModel
									  {
										  CustomerId = Convert.ToInt64(dr["CustomerID"]),
										  CustomerName = Convert.ToString(dr["CustomerName"]),
										  NumberPlate = Convert.ToString(dr["NumberPlate"]),
										  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										  TimeZone = Convert.ToString(dr["TimeZone"]),
										  BookingAmount = Convert.ToDecimal(dr["NetAmount"]),
										  EnterDate = Convert.ToDateTime(dr["EntryDate"]),
										  EnterTime = Convert.ToString(dr["EnterTime"]),
										  ExitDate = model.NotificationDateTime.Date,
										  ExitTime = Convert.ToString(model.NotificationDateTime.TimeOfDay),
										  VehicleModal = Convert.ToString(dr["VehicleModal"]),
										  LocationName = Convert.ToString(dr["LocationName"]),
										  CustomerBookingId = Convert.ToInt64(dr["CustomerBookingId"])
									  }).FirstOrDefault();
				if (VehicleRequest != null)
				{
					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(VehicleRequest.TimeZone);
					VehicleRequest.NotificationDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
				}
				return VehicleRequest;
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

		public CustomerNotificationModel GetNotificationByCustomerId(long CustomerId, string sortColumn, string sortOrder, int pageNo, int? pageSize)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetNotificationByCustomerId");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);

				DataTable dtNotification = objSQL.FetchDT(objCmd);
				if (dtNotification.Rows.Count > 0)
				{
					var lstNotification = (from DataRow dr in dtNotification.Rows
										   select new CustomerNotification
										   {
											   NotificationId = Convert.ToInt64(dr["NotificationId"]),
											   LocationName = Convert.ToString(dr["LocationName"]),
											   Message = Convert.ToString(dr["Message"]),
											   LocationPic = Convert.ToString(dr["LocationPic"]),
											   NotificationDateTime = Convert.ToDateTime(dr["NotificationDateTime"]),
											   IsBookingCompleted = Convert.ToBoolean(dr["IsBookingCompleted"]),
											   UnreadCount = Convert.ToInt64(dr["UnreadCount"]),
											   ShowActionButtons = Convert.ToBoolean(dr["ShowActionButtons"])
										   }).ToList();

					return new CustomerNotificationModel { Notifications = lstNotification.OrderByDescending(a => a.UnreadCount).ToList(), Total = Convert.ToInt32(dtNotification.Rows[0]["TotalCount"]) };
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

		public CheckCustomerDueAmount CheckCustomerDueAmount(long BookingId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CheckCustomerDueAmount");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", BookingId);

				DataTable dtCutsomer = objSQL.FetchDT(objCmd);

				var CustDetails = (from DataRow dr in dtCutsomer.Rows
								   select new CheckCustomerDueAmount
								   {
									   TotalCharges = Convert.ToDecimal(dr["TotalCharges"]),
									   PaidAmount = dr["PaidAmount"] == DBNull.Value ? 0.00m : Convert.ToDecimal(dr["PaidAmount"]),
									   TimeZoneId = Convert.ToString(dr["TimeZoneId"])
								   }).FirstOrDefault();
				CustDetails.UnpaidAmount = CustDetails.TotalCharges - CustDetails.PaidAmount;

				return CustDetails;
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
		public CustomerPaymentDetails GetCustomerPaymentDetails(long CustomerId, long ParkingLocationId, long? CustomerVehicleId = null)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerBillingDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", CustomerVehicleId);
				DataSet dbCustomer = objSQL.FetchDB(objCmd);

				var CustDetails = (from DataRow dr in dbCustomer.Tables[0].Rows
								   select new CustomerPaymentDetails
								   {
									   CustomerId = Convert.ToInt64(dr["Id"]),
									   CustomerName = Convert.ToString(dr["CustomerName"]),
									   Address = Convert.ToString(dr["Address"]),
									   State = Convert.ToString(dr["State"]),
									   Country = Convert.ToString(dr["Country"]),
									   City = Convert.ToString(dr["City"]),
									   Mobile = Convert.ToString(dr["Mobile"]),
									   CountryCode = Convert.ToString(dr["CountryCode"]),
									   PaypalCustomerId = Convert.ToString(dr["PaypalCustomerId"])
								   }).FirstOrDefault();
				if (CustDetails != null)
				{
					CustDetails.Currency = (from DataRow dr in dbCustomer.Tables[1].Rows
											select
					Convert.ToString(dr["Currency"])).FirstOrDefault();

				}
				return CustDetails;
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

		public BookingPaymentDetails GetBookingPaymentDetails(long CustomerBookingId, DateTime CurrentDate)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", CustomerBookingId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

				DataSet dsBooking = objSQL.FetchDB(objCmd);
				var bookingDetails = (from DataRow dr in dsBooking.Tables[0].Rows
									  select new BookingDetailResponse
									  {
										  Id = Convert.ToInt64(dr["Id"]),
										  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										  Name = Convert.ToString(dr["CustomerName"]),
										  Mobile = Convert.ToString(dr["Mobile"]),
										  Email = Convert.ToString(dr["Email"]),
										  Address = Convert.ToString(dr["Address"]),
										  ProfilePic = Convert.ToString(dr["ProfilePic"]),
										  Charges = Convert.ToDecimal(dr["Charges"]),
										  TotalCharges = Convert.ToInt32(dr["bookingtypeid"]).Equals(2) ? Convert.ToDecimal(dr["Charges"]) : Convert.ToDecimal(dr["NetAmount"]),
										  StartDate = Convert.ToDateTime(dr["StartDate"]),
										  EndDate = Convert.ToDateTime(dr["EndDate"]),
										  StartTime = Convert.ToString(dr["StartTime"]),
										  EndTime = Convert.ToString(dr["EndTime"]),
										  Duration = Convert.ToDecimal(dr["Duration"]),
										  NumberPlate = Convert.ToString(dr["NumberPlate"]),
										  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
										  VehicleManufacturer = dr["VehicleManufacturer"] == DBNull.Value ? null : Convert.ToString(dr["VehicleManufacturer"]),
										  VehicleColor = dr["VehicleColor"] == DBNull.Value ? null : Convert.ToString(dr["VehicleColor"]),
										  VehicleState = dr["VehicleState"] == DBNull.Value ? null : Convert.ToString(dr["VehicleState"]),
										  CustomerInfoId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
										  CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
										  EnterExitId = dr["EnterExitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["EnterExitId"]),
										  Notes = Convert.ToString(dr["Notes"]),
										  EntryDate = Convert.ToString(dr["EntryDate"]),
										  EnterTime = Convert.ToString(dr["EnterTime"]),
										  ExitDate = Convert.ToString(dr["ExitDate"]),
										  ExitTime = Convert.ToString(dr["ExitTime"]),
										  Symbol = Convert.ToString(dr["Symbol"]),
										  IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
										  TimeZone = Convert.ToString(dr["TimeZone"])

									  }).FirstOrDefault();

				objCmd = new SqlCommand("sp_ParkingInfo");
				objCmd.Parameters.AddWithValue("@ParkingLocationId", bookingDetails.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerBookingId", CustomerBookingId);
				DataSet dsInfo = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();
				StatesMst state = null; Countries country = null;

				var plocation = (from DataRow dr in dsInfo.Tables[0].Rows
								 select new ParkingLocDetailsResponse
								 {
									 Id = Convert.ToInt64(dr["Id"]),
									 LocationName = Convert.ToString(dr["LocationName"]),
									 Address = dr["Address"].ToString(),
									 City = dr["City"].ToString(),
									 StateCode = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
									 State = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
									 Country = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
									 CountryCode = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
									 ZipCode = dr["ZipCode"].ToString(),
									 LocationPic = dr["LocationPic"].ToString(),
									 Mobile = dr["Mobile"].ToString(),
									 OverSizedChargesMonthly = Convert.ToDecimal(dr["OverSizedChargesMonthly"]),
									 OverSizedChargesRegular = Convert.ToDecimal(dr["OverSizedChargesRegular"]),
									 PaymentMethod = dr["PaymentGateway"] == DBNull.Value ? null : Convert.ToString(dr["PaymentGateway"]),
									 ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
									 SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"])

								 }).FirstOrDefault();
				var PaymentDetails = (from DataRow dr in dsInfo.Tables[1].Rows
									  select new BookingPaymentDetails
									  {
										  BookingDetails = bookingDetails,
										  ParkingLocationDetails = plocation,
										  BookingAmount = Convert.ToDecimal(dr["NetAmount"]),
										  PaidAmount = dr["PaidAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["PaidAmount"])
									  }).FirstOrDefault();
				PaymentDetails.DueAmount = PaymentDetails.BookingAmount - Convert.ToDecimal(PaymentDetails.DueAmount);

				return PaymentDetails;
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

		public long DeleteVehicleDamageReport(VehicleDamageIdModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_DeleteVehicleDamageReport");
			try
			{
				objCmd.Parameters.AddWithValue("@DamageVehicleId", model.DamageVehicleId);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				return model.DamageVehicleId;
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

		public long ChangeVehicleForBooking(ChangeVehicleRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_ChangeVehicleForBooking");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);

				//DataTable dtCustomer = objSQL.FetchDT(objCmd);
				objSQL.UpdateDB(objCmd, true);
				return model.CustomerBookingId;
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


		// public ElectronicPaymentDetails GetElectronicPaymentDetails(long CustomerBookingId, DateTime CurrentDate)
		// {
		//     // var bookingDetail = GetBookingDetails(CustomerBookingId, CurrentDate);


		//     SQLManager objSQL = new SQLManager(_configuration);
		//     SqlCommand objCmd = new SqlCommand("sp_GetBookingDetails");

		//     try
		//     {
		//         objCmd.Parameters.AddWithValue("@BookingId", CustomerBookingId);
		//         objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

		//         DataSet dsBooking = objSQL.FetchDB(objCmd);


		//         var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

		//         StatesMst state = null; Countries country = null;

		//         var bookingDetail = (from DataRow dr in dsBooking.Tables[0].Rows
		//                              select new BookingDetailResponse
		//                              {
		//                                  Id = Convert.ToInt64(dr["Id"]),
		//                                  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
		//                                  Name = Convert.ToString(dr["CustomerName"]),
		//                                  Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
		//                                  MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
		//                                  Email = Convert.ToString(dr["Email"]),
		//                                  Address = Convert.ToString(dr["Address"]),
		//                                  ProfilePic = Convert.ToString(dr["ProfilePic"]),
		//                                  Charges = Convert.ToDecimal(dr["Charges"]),
		//                                  TotalCharges = Convert.ToDecimal(dr["NetAmount"]),
		//                                  StartDate = Convert.ToDateTime(dr["StartDate"]),
		//                                  EndDate = Convert.ToDateTime(dr["EndDate"]),
		//                                  StartTime = DateTime.Parse(Convert.ToString(dr["StartTime"])).ToString("HH:mm"),
		//                                  EndTime = DateTime.Parse(Convert.ToString(dr["EndTime"])).ToString("HH:mm"),
		//                                  Duration = Convert.ToDecimal(dr["Duration"]),
		//                                  NumberPlate = Convert.ToString(dr["NumberPlate"]),
		//                                  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
		//                                  VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
		//                                  VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
		//                                  VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
		//                                  CustomerInfoId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
		//                                  CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
		//                                  EnterExitId = dr["EnterExitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["EnterExitId"]),
		//                                  Notes = Convert.ToString(dr["Notes"]),
		//                                  EntryDate = Convert.ToString(dr["EntryDate"]),
		//                                  EnterTime = Convert.ToString(dr["EnterTime"]),
		//                                  ExitDate = Convert.ToString(dr["ExitDate"]),
		//                                  ExitTime = Convert.ToString(dr["ExitTime"]),
		//                                  Symbol = Convert.ToString(dr["Symbol"]),
		//                                  IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
		//                                  TimeZone = Convert.ToString(dr["TimeZone"]),
		//                                  TaxAmount = dr["Tax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Tax"]),
		//                                  MaxDurationofSlab = dr["MaxDurationofSlab"] == DBNull.Value ? (double?)null : Convert.ToDouble(dr["MaxDurationofSlab"]),
		//                                  MaxRateofSlab = dr["MaxRateofSlab"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["MaxRateofSlab"]),
		//                                  MonthlyPassMessage = Convert.ToInt32(dr["bookingtypeid"]).Equals(2) ? "(Monthly Pass)" : null,
		//                                  OverweightCharges = Convert.ToDecimal(dr["ExtraCharges"]),
		//                                  BookingTypeId = Convert.ToInt32(dr["BookingTypeId"])
		//                              }).FirstOrDefault();


		//         if (bookingDetail != null)
		//         {
		//             var extraDetails = (from DataRow dr in dsBooking.Tables[1].Rows
		//                                 select new
		//                                 {

		//                                     LastBookingDate = dr["LastBookingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["LastBookingDate"]),
		//                                     PaidAmount = Convert.ToDecimal(dr["PaidAmount"])
		//                                 }).FirstOrDefault();


		//             bookingDetail.UnpaidAmount = bookingDetail.TotalCharges - extraDetails.PaidAmount;
		//             bookingDetail.BookingType = ((EBookingType)(bookingDetail.BookingTypeId)).ToString();

		//             var minutes = (int)((bookingDetail.Duration - Math.Truncate(bookingDetail.Duration)) * 60);
		//             var duration = ((int)bookingDetail.Duration == 0 ? "" : (int)bookingDetail.Duration + " hours") + (minutes == 0 ? "" : " " + minutes + " minutes");
		//             bookingDetail.MaxStay = duration.Trim();

		//             objSQL = new SQLManager(_configuration);
		//             objCmd = new SqlCommand("sp_GetBookedLocationDetails");
		//             objCmd.Parameters.AddWithValue("@ParkingLocationId", bookingDetail.ParkingLocationId);
		//             DataTable dtLocDetails = objSQL.FetchDT(objCmd);

		//             state = null;

		//             var paymentDetails = (from DataRow dr in dtLocDetails.Rows
		//                                   select new ElectronicPaymentDetails
		//                                   {
		//                                       Id = Convert.ToInt64(dr["Id"]),
		//                                       LocationName = Convert.ToString(dr["LocationName"]),
		//                                       LocationAddress = dr["Address"].ToString(),
		//                                       City = dr["City"].ToString(),
		//                                       State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
		//                                       StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
		//                                       Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
		//                                       CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
		//                                       ZipCode = dr["ZipCode"].ToString(),
		//                                       LocationPic = dr["LocationPic"].ToString(),
		//                                       LocationMobile = dr["Mobile"].ToString(),
		//                                       PaymentMethod = dr["PaymentGateway"] == DBNull.Value ? null : Convert.ToString(dr["PaymentGateway"]),
		//                                       ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
		//                                       SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
		//                                       AccessToken = dr["AccessToken"] == DBNull.Value ? null : Convert.ToString(dr["AccessToken"]),
		//                                       IsProduction = Convert.ToBoolean(dr["IsProduction"]),
		//                                       ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
		//                                       LocationId = dr["LocationId"] == DBNull.Value ? null : Convert.ToString(dr["LocationId"]),
		//                                       TaxPercent = Convert.ToDecimal(dr["Tax"]),
		//                                       BusinessTitle = Convert.ToString(dr["BusinessTitle"])
		//                                   }).FirstOrDefault();

		//             bookingDetail.LocationName = paymentDetails.LocationName;
		//             bookingDetail.LocationAddress = paymentDetails.LocationAddress;
		//             _mapper.Map<BookingDetailResponse, ElectronicPaymentDetails>(bookingDetail, paymentDetails);

		//             paymentDetails.PaymentNotes = (paymentDetails.StartDate + TimeSpan.Parse(paymentDetails.StartTime)).ToString("MMM dd h:mm tt") + " to " + (paymentDetails.EndDate + TimeSpan.Parse(paymentDetails.EndTime)).ToString("MMM dd h:mm tt");

		//             return paymentDetails;
		//         }
		//         return null;
		//     }
		//     catch (Exception)
		//     {
		//         throw;
		//     }
		//     finally
		//     {
		//         if (objSQL != null) objSQL.Dispose();
		//         if (objCmd != null) objCmd.Dispose();
		//     }

		// }

		public ElectronicPaymentDetails GetElectronicPaymentDetails(long CustomerBookingId)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetElectronicPaymentDetails_v1");

			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", CustomerBookingId);
				DataSet dsBooking = objSQL.FetchDB(objCmd);
				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				var bookingDetail = (from DataRow dr in dsBooking.Tables[0].Rows
									 select new BookingDetailResponse
									 {
										 Id = Convert.ToInt64(dr["Id"]),
										 ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										 Name = Convert.ToString(dr["CustomerName"]),
										 Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
										 MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
										 Email = Convert.ToString(dr["Email"]),
										 Address = Convert.ToString(dr["Address"]),
										 ProfilePic = Convert.ToString(dr["ProfilePic"]),
										 Charges = Convert.ToDecimal(dr["Charges"]),
										 TotalCharges = Convert.ToDecimal(dr["NetAmount"]),
										 StartDate = Convert.ToDateTime(dr["StartDate"]),
										 EndDate = Convert.ToDateTime(dr["EndDate"]),
										 StartTime = DateTime.Parse(Convert.ToString(dr["StartTime"])).ToString("HH:mm"),
										 EndTime = DateTime.Parse(Convert.ToString(dr["EndTime"])).ToString("HH:mm"),
										 Duration = Convert.ToDecimal(dr["Duration"]),
										 CustomerInfoId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
										 CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
										 EnterExitId = dr["EnterExitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["EnterExitId"]),
										 Notes = Convert.ToString(dr["Notes"]),
										 EntryDate = Convert.ToString(dr["EntryDate"]),
										 EnterTime = Convert.ToString(dr["EnterTime"]),
										 ExitDate = Convert.ToString(dr["ExitDate"]),
										 ExitTime = Convert.ToString(dr["ExitTime"]),
										 Symbol = Convert.ToString(dr["Symbol"]),
										 IsEarlyBirdOfferApplied = Convert.ToBoolean(dr["IsEarlyBirdOfferApplied"]),
										 TimeZone = Convert.ToString(dr["TimeZone"]),
										 TaxAmount = dr["Tax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Tax"]),
										 MonthlyPassMessage = Convert.ToInt32(dr["bookingtypeid"]).Equals(2) ? "(Monthly Pass)" : null,
										 OverweightCharges = Convert.ToDecimal(dr["ExtraCharges"]),
										 BookingTypeId = Convert.ToInt32(dr["BookingTypeId"]),
										 BookingAmount = Convert.ToDecimal(dr["BookingAmount"]),
										 ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
										 PaypalCustomerId = Convert.ToString(dr["PaypalCustomerId"])
									 }).FirstOrDefault();


				if (bookingDetail != null)
				{
					var extraDetails = (from DataRow dr in dsBooking.Tables[1].Rows
										select new
										{
											LastBookingDate = dr["LastBookingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["LastBookingDate"]),
											PaidAmount = Convert.ToDecimal(dr["PaidAmount"])
										}).FirstOrDefault();

					bookingDetail.PaidAmount = extraDetails.PaidAmount;
					// bookingDetail.UnpaidAmount = bookingDetail.TotalCharges - extraDetails.PaidAmount;
					bookingDetail.BookingType = ((EBookingType)(bookingDetail.BookingTypeId)).ToString();

					var minutes = (int)((bookingDetail.Duration - Math.Truncate(bookingDetail.Duration)) * 60);
					var duration = ((int)bookingDetail.Duration == 0 ? "" : (int)bookingDetail.Duration + " hours") + (minutes == 0 ? "" : " " + minutes + " minutes");
					bookingDetail.MaxStay = duration.Trim();

					var paymentDetails = (from DataRow dr in dsBooking.Tables[2].Rows
										  select new ElectronicPaymentDetails
										  {
											  Id = Convert.ToInt64(dr["Id"]),
											  LocationName = Convert.ToString(dr["LocationName"]),
											  LocationAddress = dr["Address"].ToString(),
											  City = dr["City"].ToString(),
											  State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
											  StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
											  Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
											  CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
											  ZipCode = dr["ZipCode"].ToString(),
											  LocationPic = dr["LocationPic"].ToString(),
											  LocationMobile = dr["Mobile"].ToString(),
											  PaymentMethod = dr["PaymentGateway"] == DBNull.Value ? null : Convert.ToString(dr["PaymentGateway"]),
											  ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
											  SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
											  AccessToken = dr["AccessToken"] == DBNull.Value ? null : Convert.ToString(dr["AccessToken"]),
											  IsProduction = Convert.ToBoolean(dr["IsProduction"]),
											  ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
											  LocationId = dr["LocationId"] == DBNull.Value ? null : Convert.ToString(dr["LocationId"]),
											  TaxPercent = Convert.ToDecimal(dr["Tax"]),
											  BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
											  ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
										  }).FirstOrDefault();

					var taxAmt = parkingHelper.RoundOff(((bookingDetail.BookingAmount + bookingDetail.OverweightCharges + paymentDetails.ConvenienceFee) * paymentDetails.TaxPercent) / 100);

					bookingDetail.TaxAmount = taxAmt;
					bookingDetail.LocationName = paymentDetails.LocationName;
					bookingDetail.LocationAddress = paymentDetails.LocationAddress;
					bookingDetail.TotalCharges = bookingDetail.ConvenienceFee > 0 ? bookingDetail.BookingAmount + bookingDetail.OverweightCharges + taxAmt : bookingDetail.BookingAmount + bookingDetail.OverweightCharges + taxAmt + paymentDetails.ConvenienceFee;
					// bookingDetail.TotalCharges = bookingDetail.BookingAmount + bookingDetail.OverweightCharges + paymentDetails.ConvenienceFee + taxAmt;
					bookingDetail.ConvenienceFee = paymentDetails.ConvenienceFee;
					bookingDetail.UnpaidAmount = bookingDetail.TotalCharges - extraDetails.PaidAmount;
					bookingDetail.PaymentMethod = paymentDetails.PaymentMethod;
					bookingDetail.LocationId = paymentDetails.LocationId;
					bookingDetail.AccessToken = paymentDetails.AccessToken;
					bookingDetail.SecretKey = paymentDetails.SecretKey;
					bookingDetail.ApiKey = paymentDetails.ApiKey;
					bookingDetail.LocationId = paymentDetails.LocationId;
					bookingDetail.IsProduction = paymentDetails.IsProduction;
					_mapper.Map<BookingDetailResponse, ElectronicPaymentDetails>(bookingDetail, paymentDetails);

					paymentDetails.PaymentNotes = (paymentDetails.StartDate + TimeSpan.Parse(paymentDetails.StartTime)).ToString("MMM dd h:mm tt") + " to " + (paymentDetails.EndDate + TimeSpan.Parse(paymentDetails.EndTime)).ToString("MMM dd h:mm tt");

					return paymentDetails;
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

		public void MakeElectronicPayment(ElectronicPaymentRequest model, object PaymentInfo = null)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_MakeElectronicPayment_v2");
			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
				objCmd.Parameters.AddWithValue("@UnpaidAmount", model.UnpaidAmount);
				objCmd.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
				objCmd.Parameters.AddWithValue("@Currency", model.Currency);
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
				objCmd.Parameters.AddWithValue("@PaymentNotes", model.PaymentNotes);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
				objCmd.Parameters.AddWithValue("@ConvenienceFee", model.ConvenienceFee);
				objCmd.Parameters.AddWithValue("@TotalCharges", model.TotalCharges);
				objCmd.Parameters.AddWithValue("@TaxAmount", model.TaxAmount);

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
		}

		public List<VehicleListResponse> GetParkedVehicleListByCustomerId(long CustomerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkedVehicleListByCustomerId");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);

				DataTable dtVehicle = objSQL.FetchDT(objCmd);
				var lstVehicles = (from DataRow dr in dtVehicle.Rows
								   select new VehicleListResponse
								   {
									   CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
									   NumberPlate = Convert.ToString(dr["NumberPlate"])
								   }).ToList();
				return lstVehicles;
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

		public CustomerBookingDetailsResponse GetBookingDetailsByCustomer(long BookingId)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetBookingDetailsByCustomer");

			try
			{
				objCmd.Parameters.AddWithValue("@BookingId", BookingId);
				DataSet dsBooking = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				var bookingDetails = (from DataRow dr in dsBooking.Tables[0].Rows
									  select new CustomerBookingDetailsResponse
									  {
										  Id = Convert.ToInt64(dr["Id"]),
										  ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
										  LocationName = Convert.ToString(dr["LocationName"]),
										  Address = Convert.ToString(dr["Address"]),
										  City = Convert.ToString(dr["City"]),
										  StateCode = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
										  State = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
										  Country = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
										  CountryCode = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
										  ZipCode = Convert.ToString(dr["ZipCode"]),
										  LocationPic = Convert.ToString(dr["LocationPic"]),
										  Mobile = Convert.ToString(dr["Mobile"]),
										  Latitude = Convert.ToString(dr["Latitude"]),
										  Longitude = Convert.ToString(dr["Longitude"]),
										  Charges = Convert.ToDecimal(dr["Charges"]),
										  TotalCharges = Convert.ToDecimal(dr["NetAmount"]),
										  StartDate = Convert.ToDateTime(dr["StartDate"]),
										  EndDate = Convert.ToDateTime(dr["EndDate"]),
										  StartTime = Convert.ToString(dr["StartTime"]),
										  EndTime = Convert.ToString(dr["EndTime"]),
										  Duration = Convert.ToDecimal(dr["Duration"]),
										  NumberPlate = Convert.ToString(dr["NumberPlate"]),
										  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
										  VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
										  VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
										  VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : VehicleMasterData.ListStates.Find(a => a.Id == Convert.ToInt64(dr["VehicleStateId"])).Name,
										  CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
										  TaxAmount = dr["Tax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Tax"]),
										  ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"]),
										  BookingCategoryId = Convert.ToInt32(dr["BookingCategoryId"]),
										  BookingTypeId = Convert.ToInt32(dr["BookingTypeId"])
									  }).FirstOrDefault();


				if (bookingDetails != null)
				{
					var minutes = (int)((bookingDetails.Duration - Math.Truncate(bookingDetails.Duration)) * 60);
					var duration = ((int)bookingDetails.Duration == 0 ? "" : (int)bookingDetails.Duration + " hours") + (minutes == 0 ? "" : " " + minutes + " minutes");
					bookingDetails.MaxStay = duration.Trim();


				}
				return bookingDetails;
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



		public void InsertIntoLogFile(string Message)
		{
			try
			{


				StreamWriter sw = null;
				using (sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\Log.txt", true))
				{
					sw.WriteLine("\n" + DateTime.Now.ToString() + " : " + Message.Trim());
					sw.WriteLine("--------------------------------------");
					sw.Flush();
					sw.Close();
				}



				// string fileName = Directory.GetCurrentDirectory() + "\\Log.txt";

				// if (File.Exists(fileName))
				// {
				//     File.Delete(fileName);
				// }


				// using (StreamWriter sw = File.CreateText(fileName))
				// {
				//     sw.WriteLine(Message);
				//     sw.WriteLine("-----------------------------------------------");
				// }

				// // Open the stream and read it back.    
				// using (StreamReader sr = File.OpenText(fileName))
				// {
				//     string s = "";
				//     while ((s = sr.ReadLine()) != null)
				//     {
				//         Console.WriteLine(s);
				//     }
				// }
			}
			catch (Exception Ex)
			{
				Console.WriteLine(Ex.ToString());
			}
		}


		public (long, string) SetGuestOtp(SetGuestOtpRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_SetCustomerOtp");
			try
			{
				Random r = new Random();
				string OTP = r.Next(1000, 9999).ToString();
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@OTP", OTP);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var customerInfo = (from DataRow dr in dtCustomer.Rows
									select new
									{
										CustomerId = Convert.ToInt64(dr["CustomerId"]),
										Mobile = Convert.ToString(dr["Mobile"])
									}).FirstOrDefault();

				if (customerInfo == null)
					throw new AppException("Guest doesn't exist");

				string MsgText = $"Your Flix Valet App otp is: {OTP}";

				_smsService.SendSMS(MsgText, model.Mobile);

				return (customerInfo.CustomerId, OTP);
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

		public void UpdateNotificationStatus(UpdateNotificationStatusModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateNotificationStatus");
			try
			{
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@NotificationId", model.NotificationId);
				DataTable dtError = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtError.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);
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


		public long AddNotificationbyStaff(StaffNotificationModel model, NotificationDetails NotificationDetails)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddNotificationByStaff");
			try
			{
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@CustomerId", NotificationDetails.CustomerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", NotificationDetails.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@NotificationMessage", model.NotificationMessage);
				objCmd.Parameters.AddWithValue("@NotificationDateTime", model.NotificationDateTime);

				DataTable dtNotificationId = objSQL.FetchDT(objCmd);

				if (dtNotificationId.Rows.Count > 0)
					return Convert.ToInt64(dtNotificationId.Rows[0]["NotificationId"]);
				return 0;
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

		public NotificationDetails GetNotificationDetails(StaffNotificationModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetNotificationDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@NotificationId", model.NotificationId);
				DataSet ds = objSQL.FetchDB(objCmd);

				var NotificationDetails = (from DataRow dr in ds.Tables[0].Rows
										   select new NotificationDetails
										   {
											   DeviceTokens = Convert.ToString(dr["Tokens"]).Split(','),
											   CustomerId = Convert.ToInt64(dr["CustomerId"]),
											   ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
											   TimeZone = Convert.ToString(dr["TimeZone"]),
											   NumberPlate = Convert.ToString(dr["NumberPlate"])
										   }).FirstOrDefault();

				if (NotificationDetails != null)
				{
					long BadgeCount = (from DataRow dr in ds.Tables[1].Rows
									   select Convert.ToInt64(dr["BadgeCount"])).FirstOrDefault();

					NotificationDetails.BadgeCount = BadgeCount;
				}
				return NotificationDetails;
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

		public long StaffCustConversation(StaffNotificationModel model, NotificationDetails NotificationDetails)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_StaffCustConversation");
			try
			{
				objCmd.Parameters.AddWithValue("@NotificationId", model.NotificationId);
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@CustomerId", NotificationDetails.CustomerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", NotificationDetails.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@IsFromCustomer", model.IsFromCustomer);
				objCmd.Parameters.AddWithValue("@NotificationMessage", model.NotificationMessage);
				objCmd.Parameters.AddWithValue("@NotificationDateTime", model.NotificationDateTime);

				DataTable dtNotificationId = objSQL.FetchDT(objCmd);

				if (dtNotificationId.Rows.Count > 0)
					return Convert.ToInt64(dtNotificationId.Rows[0]["NotificationId"]);
				return 0;
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

		public RecognizedVehicleListResponse GetRecognizedVehicleList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, DateTime CurrentDate, string CameraId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetRecognizedVehicleListByOwner");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				objCmd.Parameters.AddWithValue("@PageNo", pageNo);
				objCmd.Parameters.AddWithValue("@PageSize", pageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				objCmd.Parameters.AddWithValue("@CameraId", CameraId);

				DataTable dtVehicle = objSQL.FetchDT(objCmd);
				if (dtVehicle.Rows.Count > 0)
				{
					var listBookings = (from DataRow dr in dtVehicle.Rows
										select new RecognizedVehicleListByOwner
										{
											Id = Convert.ToInt64(dr["Id"]),
											NumberPlate = Convert.ToString(dr["NumberPlate"]),
											ReportedDate = Convert.ToDateTime(dr["ReportedDate"]),
											CustomerBookingId = dr["CustomerBookingId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerBookingId"]),
											CustomerType = (dr["CustomerType"].ToString()) == "" ? "-" : Convert.ToString(dr["CustomerType"]),
											CustomerName = (dr["CustomerName"].ToString()) == " " ? "-" : Convert.ToString(dr["CustomerName"]),
										}).ToList();

					return new RecognizedVehicleListResponse
					{
						RecognizedVehicleList = listBookings,
						Total = Convert.ToInt32(dtVehicle.Rows[0]["TotalCount"])
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

		public GuestIDResponse AddGuestDetails(GuestDetailsRequest model, string Otp)
		{

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddGuestDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@NumberPlate", model.NumberPlate);
				objCmd.Parameters.AddWithValue("@VehicleModal", model.VehicleModal);
				objCmd.Parameters.AddWithValue("@VehicleTypeId", model.VehicleTypeId);
				objCmd.Parameters.AddWithValue("@VehicleColorId", model.VehicleColorId);
				objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.VehicleManufacturerId);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@Otp", Otp);
				DataTable dtGuest = objSQL.FetchDT(objCmd);

				var bookingDetails = (from DataRow dr in dtGuest.Rows
									  select new GuestIDResponse
									  {
										  CustomerId = Convert.ToInt64(dr["CustomerId"]),
										  CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"])
									  }).FirstOrDefault();
				return bookingDetails;
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

		public GuestList FetchGuestfromVehicle(string NumberPlate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_FetchGuestfromVehicle");
			try
			{
				objCmd.Parameters.AddWithValue("@NumberPlate", NumberPlate);
				DataTable dtGuest = objSQL.FetchDT(objCmd);

				if (dtGuest.Rows.Count > 0)
				{
					GuestList guestList = new GuestList();
					var GuestDetails = (from DataRow dr in dtGuest.Rows
										select new Guest
										{
											CustomerId = Convert.ToInt64(dr["CustomerId"]),
											CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
											HasMobile = Convert.ToBoolean(dr["HasMobile"]),
											Mobile = dr["Mobile"] == DBNull.Value ? null : parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
											MobileCode = dr["Mobile"] == DBNull.Value ? null : parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
											CreatedDate = Convert.ToDateTime(dr["CreatedDate"])
										}).ToList();
					if (GuestDetails.Any(a => !a.HasMobile))
						guestList.WithoutNumber = GuestDetails.FirstOrDefault(a => !a.HasMobile);
					if (GuestDetails.Any(a => a.HasMobile))
						guestList.WithNumber = GuestDetails.OrderByDescending(a => a.CreatedDate).FirstOrDefault(a => a.HasMobile);
					return guestList;

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

		public List<VehicleBookingResponse> ScanReceipt(long CustomerBookingId, bool IsExit)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_ScanReceipt");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerBookingId", CustomerBookingId);
				objCmd.Parameters.AddWithValue("@IsExit", IsExit);

				DataTable dtBooking = objSQL.FetchDT(objCmd);

				var bookingResponse = (from DataRow dr in dtBooking.Rows
									   select new VehicleBookingResponse
									   {
										   BookingId = Convert.ToInt64(dr["Id"]),
										   EntryDate = Convert.ToString(dr["EntryDate"]),
										   ExitDate = Convert.ToString(dr["ExitDate"]),
										   EnterTime = Convert.ToString(dr["EnterTime"]),
										   ExitTime = Convert.ToString(dr["ExitTime"]),
										   BookingTypeId = Convert.ToInt32(dr["BookingTypeId"])
									   }).ToList();
				return bookingResponse;
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

		public (long, long) BrowserLaunch(BrowserLaunchRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_BrowserLaunch");
			try
			{
				objCmd.Parameters.AddWithValue("@UserId", model.UserId);
				objCmd.Parameters.AddWithValue("@BrowserDeviceToken", model.BrowserDeviceToken);
				DataTable dtBrowser = objSQL.FetchDT(objCmd);
				return (model.UserId, Convert.ToInt64(dtBrowser.Rows[0]["BadgeCount"]));
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

		public CustomerDetailsResponse GetCustomerDetails(long CustomerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetCustomerDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerId", CustomerId);
				DataSet ds = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;
				if (ds.Tables[0].Rows.Count > 0)
				{

					var CustInfo = (from DataRow dr in ds.Tables[0].Rows
									select new CustomerDetailsResponse
									{
										UserId = Convert.ToInt64(dr["UserId"]),
										CustomerId = Convert.ToInt64(dr["CustomerId"]),
										FirstName = Convert.ToString(dr["FirstName"]),
										LastName = Convert.ToString(dr["LastName"]),
										Email = Convert.ToString(dr["Email"]),
										Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
										MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
										Address = Convert.ToString(dr["Address"]),
										StateCode = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
										State = dr["StateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
										CountryCode = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
										Country = dr["CountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
										City = Convert.ToString(dr["City"]),
										ZipCode = Convert.ToString(dr["ZipCode"]),
										ProfilePic = Convert.ToString(dr["ProfilePic"]),
										Gender = (!string.IsNullOrEmpty(dr["Gender"].ToString())) ? char.Parse(dr["Gender"].ToString()) : 'U'
									}).FirstOrDefault();

					var vehicleDetails = (from DataRow dr in ds.Tables[1].Rows
										  select new VehicleDetails
										  {
											  Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
											  NumberPlate = Convert.ToString(dr["NumberPlate"]),
											  VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"])
										  }).ToList();

					CustInfo.CustomerVehicles = vehicleDetails;

					return CustInfo;
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

		public PreBookingDetailsResponse GetPreBookingDetails(long ParkingLocationId, long CustomerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationDetails");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
				DataSet ds = objSQL.FetchDB(objCmd);

				var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				if (ds.Tables[1].Rows.Count > 0)
				{
					var plocation = (from DataRow dr in ds.Tables[1].Rows
									 select new ParkingLocationDetails
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
										 ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"])
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
					//Fetching Customer details
					var customerDetails = GetCustomerDetails(CustomerId);
					return new PreBookingDetailsResponse
					{
						ParkingLocationDetails = plocation,
						CustomerDetails = customerDetails
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

		public async Task PostBookingActions(PostBookingModel postBookingModel, BookingRequest bookingRequest, decimal Amount)
		{
			try
			{

				string CustomerMessage = string.Empty, ValetMessage = string.Empty, Message = string.Empty, StaffMessage = string.Empty;
				bool SaveValetNotification = false, IsNotificationRequired = false;

				DateTime StartDate = bookingRequest.StartDate + TimeSpan.Parse(bookingRequest.StartTime);
				DateTime EndDate = bookingRequest.EndDate + TimeSpan.Parse(bookingRequest.EndTime);

				Message = string.Format(postBookingModel.CustomerMessage, postBookingModel.NumberPlate, _dateTimeHelper.GetDateWithTimeFormat(StartDate), _dateTimeHelper.GetDateWithTimeFormat(EndDate));

				CustomerMessage = bookingRequest.BookingCategoryId == (int)EBookingCategories.ChargeBack ? Message : Amount > 0 ? $"{Message}, amount paid is {postBookingModel.Symbol}{Amount}." : $"{Message}.";


                StaffMessage = string.Format(postBookingModel.StaffMessage, postBookingModel.NumberPlate, _dateTimeHelper.GetDateWithTimeFormat(StartDate), _dateTimeHelper.GetDateWithTimeFormat(EndDate));

                ValetMessage = ((bookingRequest.PaymentMode?.ToLower() ?? string.Empty).Equals(EPaymentMode.PayLater.ToString().ToLower()))? StaffMessage: ((bookingRequest.BookingCategoryId == (int)EBookingCategories.ChargeBack)? StaffMessage: Amount > 0 ? $"{StaffMessage}, amount paid is {postBookingModel.Symbol}{Amount}." : $"{StaffMessage}");

				/* Send Notifications to Customer */
				IsNotificationRequired = SendBookingConfirmationtoCustomer(postBookingModel, bookingRequest, CustomerMessage, Message);

				/* Send Notifications to Owner and Staff */
				SaveValetNotification = SendBookingConfirmationtoOwnerStaff(postBookingModel, bookingRequest, ValetMessage, ref IsNotificationRequired);

				// /* Generate QR for Booking Id */
				// var QRResponse = _qRRepo.GetCompressedStaticTigerQRImage(Convert.ToString(postBookingModel.BookingId), bookingRequest.LogoUrl);

				// var QRCodePath = await _aWSService.UploadFile(QRResponse.data);

				PostBookingSaveModel notification = new PostBookingSaveModel
				{
					ParkingLocationId = bookingRequest.ParkingLocationId,
					CustomerBookingId = postBookingModel.BookingId,
					CustomerId = postBookingModel.CustomerId,
					CustomerMessage = CustomerMessage,
					ValetMessage = ValetMessage,
					NotificationDateTime = bookingRequest.CurrentDate,
					ValetNotificationType = ENotificationType.QRBookingConfirmation.ToString(),
					CustomerNotificationType = !bookingRequest.IsFromQRScan ? ENotificationType.CustomerBookingConfirmation.ToString() : ENotificationType.CustomerQRBookingConfirmation.ToString(),
					// QRCodePath = QRCodePath,
					SaveNotification = IsNotificationRequired,
					SaveValetNotification = SaveValetNotification
				};

				SavePostBookingDetails(notification);

			}
			catch (Exception ex)
			{
				InsertIntoLogFile(ex.Message);
			}
		}

		public void SavePostBookingDetails(PostBookingSaveModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_SavePostBookingDetailsv2");
			try
			{

				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@CustomerNotificationType", model.CustomerNotificationType);
				objCmd.Parameters.AddWithValue("@ValetNotificationType", model.ValetNotificationType);
				objCmd.Parameters.AddWithValue("@NotificationDateTime", model.NotificationDateTime);
				objCmd.Parameters.AddWithValue("@CustomerMessage", model.CustomerMessage);
				objCmd.Parameters.AddWithValue("@ValetMessage", model.ValetMessage);
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);
				objCmd.Parameters.AddWithValue("@SaveValetNotification", model.SaveValetNotification);
				//  objCmd.Parameters.AddWithValue("@QRCodePath", model.QRCodePath);
				objCmd.Parameters.AddWithValue("@SaveNotification", model.SaveNotification);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

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




		public bool SendBookingConfirmationtoCustomer(PostBookingModel postBookingModel, BookingRequest bookingRequest, string CustomerMessage, string Message)
		{
			bool IsNotificationRequired = false;
			if (!bookingRequest.IsGuestAddRequired)
			{
				string Title = postBookingModel.CustomerTitle;

				_firebaseRepo.SendNotificationtoCustomer(new string[] { postBookingModel.DeviceTokens.CustomerTokens.DeviceToken, postBookingModel.DeviceTokens.CustomerTokens.BrowserDeviceToken }, 0, Title, (bookingRequest.PaymentMode?.ToLower() ?? string.Empty).Equals(EPaymentMode.PayLater.ToString().ToLower()) ? Message : CustomerMessage);
				IsNotificationRequired = true;
			}


			if (!string.IsNullOrEmpty(bookingRequest.Mobile))
			{
				try
				{
					_smsService.SendSMS((bookingRequest.PaymentMode?.ToLower() ?? string.Empty).Equals(EPaymentMode.PayLater.ToString().ToLower()) ? (bookingRequest.IsPayLaterModeFromAdmin || bookingRequest.IsFromQRScan ?bookingRequest.TotalAmount > 0 ?  Message + $". Pay clicking -{postBookingModel.ElectronicPaymentMessage} ": Message : Message) : CustomerMessage, bookingRequest.Mobile);
				}
				catch (Exception)
				{
					//do nothing
				}
			}


			if (!string.IsNullOrEmpty(bookingRequest.Email))
			{
				_emailService.Send(
				to: bookingRequest.Email,
				subject: $"Booking Confirmation : {postBookingModel.LocationName}",
				html: bookingRequest.IsPayLaterModeFromAdmin || (bookingRequest.PaymentMode?.ToLower() ?? string.Empty).Equals(EPaymentMode.PayLater.ToString().ToLower()) ? bookingRequest.TotalAmount > 0 ? $@"<p>{Message + $@". Make payment by clicking on the link - <a href='{postBookingModel.ElectronicPaymentMessage}'>{postBookingModel.ElectronicPaymentMessage}</a> "}</p>" : $@"<p>{Message}</p>"
                : $@"<p>{CustomerMessage}</p>");
			}

			return IsNotificationRequired;
		}

		public bool SendBookingConfirmationtoOwnerStaff(PostBookingModel postBookingModel, BookingRequest bookingRequest, string ValetMessage, ref bool IsNotificationRequired)
		{
			bool SaveValetNotification = false;
			string Title = string.Empty;


			/*Notification should be sent to staff only when a customer scans location QR code onsite and creates a booking.*/

			if (bookingRequest.IsFromQRScan)
			{
				Title = string.Format(postBookingModel.StaffTitle, (bookingRequest.BookingType.ToLower().Equals("hourly") ? "Regular Transit" : "Monthly Mode"), postBookingModel.NumberPlate);
				_firebaseRepo.SendNotificationtoStaff(bookingRequest.ParkingLocationId, Title, ValetMessage, bookingRequest.CurrentDate);

				// IsNotificationRequired = true;
				SaveValetNotification = true;
			}

			/*Email sent to ParkingOwner and Supervisor to whom the location is assigned.*/
			postBookingModel.ListOwnerSupervisors.ForEach(i => _emailService.Send(
			to: i,
			subject: $"Booking Confirmation at Location : {postBookingModel.LocationName}",
			html: $@"<p>{ValetMessage}</p>"));

			return SaveValetNotification;
		}

		public GuestListv1 FetchGuestfromVehiclev1(string NumberPlate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_FetchGuestfromVehicle");
			try
			{
				objCmd.Parameters.AddWithValue("@NumberPlate", NumberPlate);
				DataTable dtGuest = objSQL.FetchDT(objCmd);

				if (dtGuest.Rows.Count > 0)
				{
					GuestListv1 guestList = new GuestListv1();
					var GuestDetails = (from DataRow dr in dtGuest.Rows
										select new Guest
										{
											CustomerId = Convert.ToInt64(dr["CustomerId"]),
											CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
											HasMobile = Convert.ToBoolean(dr["HasMobile"]),
											Mobile = dr["Mobile"] == DBNull.Value ? null : parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
											MobileCode = dr["Mobile"] == DBNull.Value ? null : parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
											CreatedDate = Convert.ToDateTime(dr["CreatedDate"])
										}).ToList();
					if (GuestDetails.Any(a => !a.HasMobile))
						guestList.WithoutNumber = GuestDetails.FirstOrDefault(a => !a.HasMobile);
					if (GuestDetails.Any(a => a.HasMobile))
						guestList.WithNumber = GuestDetails.Where(a => a.HasMobile).Select(a => a).OrderByDescending(a => a.CreatedDate).ToList();
					return guestList;
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

		public FetchCustomerDetailsResponse FetchCustomerDetails(FetchCustomerDetailsRequest model, string origin)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_FetchCustomerDetailsv4");
			try
			{
				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());
				string WeekDay = model.StartDate.DayOfWeek.ToString();


				DateTime StartdateUtc = TimeZoneInfo.ConvertTimeToUtc(model.StartDate, timeZoneInfo);

				CurrentLocationRequest request = new CurrentLocationRequest
				{
					StartDate = model.StartDate,
					StartTime = model.StartTime,
					EndDate = model.EndDate,
					EndTime = model.EndTime,
					IsFullTimeBooking = model.IsFullTimeBooking,
				};

				var (TotalHours1, SearchList) = parkingHelper.GetSearchDateTimingwiseTable(request, timeZoneInfo);
				objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@Email", model.Email);
				objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
				objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
				objCmd.Parameters.AddWithValue("@LastName", model.LastName);
				objCmd.Parameters.AddWithValue("@BookingType", model.BookingType);
				objCmd.Parameters.AddWithValue("@SearchParkingSlots", MapDataTable.ToDataTable(SearchList));

				DataSet ds = objSQL.FetchDB(objCmd);

				var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);

				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;
				if (ds.Tables[0].Rows.Count > 0)
				{
					var custInfo = (from DataRow dr in ds.Tables[1].Rows
									select new FetchCustomerDetailsResponse
									{
										CustomerId = Convert.ToInt64(dr["CustomerId"]),
									}).FirstOrDefault();

					var vehicleList = (from DataRow dr in ds.Tables[2].Rows
									   select new CustomerVehicleList
									   {
										   Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
										   CustomerInfoId = dr["CustomerInfoId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerInfoId"]),
										   NumberPlate = Convert.ToString(dr["NumberPlate"]),
										   VehicleModal = Convert.ToString(dr["VehicleModal"]),
										   VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"])
									   }).ToList();

					var location = (from DataRow dr in ds.Tables[3].Rows
									select new
									{
										Id = Convert.ToInt64(dr["Id"]),
									}).ToList();

					if (location.Count != SearchList.Count)
						throw new AppException("Parking is unavailable for provided date/time");


					var plocation = (from DataRow dr in ds.Tables[4].Rows
									 select new ParkingLocationDetailsResponse
									 {
										 Id = Convert.ToInt64(dr["Id"]),
										 Address = dr["Address"].ToString(),
										 City = dr["City"].ToString(),
										 State = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
										 StateCode = parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
										 Country = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
										 CountryCode = parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
										 ZipCode = Convert.ToString(dr["ZipCode"]),
										 LocationPic = Convert.ToString(dr["LocationPic"]),
										 Currency = Convert.ToString(dr["Currency"]),
										 Symbol = Convert.ToString(dr["Symbol"]),
										 TaxPercent = Convert.ToDecimal(dr["Tax"]),
										 OverSizedChargesMonthly = Convert.ToDecimal(dr["OverSizedChargesMonthly"]),
										 OverSizedChargesRegular = Convert.ToDecimal(dr["OverSizedChargesRegular"]),
										 LogoUrl = Convert.ToString(dr["LogoUrl"]),
										 ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
									 }).FirstOrDefault();

					if (plocation == null)
						throw new AppException("Location Not found");

					var rates = (from DataRow dr in ds.Tables[5].Rows
								 select new ParkingLocationRateRequest
								 {
									 Duration = model.BookingType.ToLower() == "monthly" ? Convert.ToInt32(dr["DurationUpto"]) * 30 : Convert.ToInt32(dr["DurationUpto"]),
									 Charges = Convert.ToDecimal(dr["Rate"])
								 }).ToList();

					if (rates == null || rates.Count == 0)
						throw new AppException("Rates aren't specified for the provided location.");

					if (!model.CustomerId.HasValue)
					{
						string ResetToken = randomTokenString();
						DateTime ResetTokenExpires = DateTime.UtcNow.AddDays(1);
						objCmd = new SqlCommand("sp_SetResetTokenForForgotPwd");
						objCmd.Parameters.AddWithValue("@Email", model.Email);
						objCmd.Parameters.AddWithValue("@ResetToken", ResetToken);
						objCmd.Parameters.AddWithValue("@ResetTokenExpires", ResetTokenExpires);
						objCmd.Parameters.AddWithValue("@IsFromCustomerApp", true);
						objSQL.UpdateDB(objCmd, true);

						sendPasswordResetEmail(model.FirstName, model.LastName, model.Email, ResetToken, origin);

					}

					var (TotalHours, PerHourRate, rate) = parkingHelper.GetTotalHoursandAmountByDuration(rates, DateTime.Parse(model.StartDate.ToShortDateString()), DateTime.Parse(model.EndDate.ToShortDateString()), model.StartTime, model.EndTime, model.IsFullTimeBooking);


					decimal TotalAmount = model.IsFullTimeBooking ? rate.Charges : parkingHelper.RoundOff(PerHourRate * TotalHours);


					plocation.PerHourRate = PerHourRate;
					plocation.TotalHours = model.BookingType.ToLower() == "monthly" ? Math.Round(TotalHours): TotalHours;
					plocation.TotalAmount = TotalAmount;
					plocation.FinalAmount = TotalAmount;
					plocation.OverSizedCharges = plocation.OverSizedCharges ?? 0.00m;
					plocation.MaxDurationofSlab = rate.Duration;
					plocation.MaxRateofSlab = rate.Charges;

					if (model.CustomerVehicleId != null && vehicleList.Find(a => a.Id.Equals(model.CustomerVehicleId) && a.VehicleTypeId.Equals(2)) != null)
					{
						plocation.OverSizedCharges = model.BookingType.ToLower().Equals("hourly") ? plocation.OverSizedChargesRegular : plocation.OverSizedChargesMonthly;
						plocation.IsOverSizedVehicle = true;
						plocation.FinalAmount += Convert.ToDecimal(plocation.OverSizedCharges);
					}
					//plocation.FinalAmount += plocation.ConvenienceFee;
					plocation.TaxAmount = parkingHelper.RoundOff((plocation.TaxPercent > 0.00m ? ((plocation.FinalAmount * plocation.TaxPercent) / 100) : 0.00m));
					plocation.FinalAmount += plocation.TaxAmount;
					plocation.FinalAmountWithConvenienceFee = plocation.FinalAmount + plocation.ConvenienceFee;
					plocation.StartDate = model.StartDate;
					plocation.EndDate = model.EndDate;
					plocation.StartTime = model.StartTime;
					plocation.EndTime = model.EndTime;


					var minutes = (int)((plocation.TotalHours - Math.Truncate(plocation.TotalHours)) * 60);
					var duration = (int)plocation.TotalHours + " hours" + (minutes == 0 ? "" : " " + minutes + " minutes");

					custInfo.CustomerVehicles = vehicleList.Where(e => e.NumberPlate.ToLower() != "unknown").ToList();
					custInfo.ParkingLocationDetails = plocation;
					return custInfo;

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

		private void sendPasswordResetEmail(string FirstName, string LastName, string Email, string ResetToken, string origin = "")
		{

			string redirectURL;
			redirectURL = $"{origin}/reset-password?Token={ResetToken}";
			string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\PasswordSet.html");
			MailText = string.Format(MailText, FirstName.Trim(), LastName.Trim(), _appSettings.AppName, redirectURL, Email);

			_emailService.Send(
				to: Email,
				subject: $"Set password request for {_appSettings.AppName}",
				html: $@"<h4>Set Password Email</h4>
						{MailText} "
			);
		}
		private string randomTokenString()
		{
			var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
			var randomBytes = new byte[40];
			rngCryptoServiceProvider.GetBytes(randomBytes);
			// convert random bytes to hex string
			return BitConverter.ToString(randomBytes).Replace("-", "");
		}
		public ExtendBookingDetailsResponse GetExtendBookingDetails(long CustomerBookingId, long CustomerId, DateTime CurrentDate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetExtendBookingDetailsv1");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerBookingId", CustomerBookingId);
				objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
				DataSet ds = objSQL.FetchDB(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;

				if (ds.Tables[0].Rows.Count > 0)
				{
					var customerId = (from DataRow dr in ds.Tables[0].Rows
									  select new
									  {
										  CustomerId = Convert.ToInt64(dr["CustomerId"])
									  }).FirstOrDefault();

					if (CustomerId != customerId.CustomerId)
						throw new AppException("Seems like you are trying to extend booking from a different account. Kindly login with the account associated with this booking or visit customer site on https://flixvalet.com/ to create a new booking.");

					var plocation = (from DataRow dr in ds.Tables[1].Rows
									 select new ParkingLocationDetails
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
										 ConvenienceFee = Convert.ToDecimal(dr["ConvenienceFee"])
									 }).FirstOrDefault();

					var vehicleList = (from DataRow dr in ds.Tables[2].Rows
									   select new CustomerVehicleList
									   {
										   Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
										   CustomerInfoId = dr["CustomerInfoId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerInfoId"]),
										   NumberPlate = Convert.ToString(dr["NumberPlate"]),
										   VehicleModal = Convert.ToString(dr["VehicleModal"]),
										   VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"])
									   }).ToList();

					var bookingDetails = (from DataRow dr in ds.Tables[3].Rows
										  select new BookingDetailsResponse
										  {
											  StartDate = Convert.ToDateTime(dr["StartDate"]),
											  EndDate = Convert.ToDateTime(dr["EndDate"]),
											  StartTime = Convert.ToString(dr["StartTime"]),
											  EndTime = Convert.ToString(dr["EndTime"]),
											  ExistingVehicleId = Convert.ToInt64(dr["CustomerVehicleId"])
										  }).FirstOrDefault();



					return new ExtendBookingDetailsResponse
					{
						BookingDetails = bookingDetails,
						ParkingLocationDetails = plocation,
						CustomerVehicles = vehicleList,
						IsBookingExists = true ? bookingDetails != null : false
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

		public SearchCustomersFromFilterResponse SearchCustomersFromFilter(string Email, string Mobile)
		{

			if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Mobile))
				return new SearchCustomersFromFilterResponse
				{
					CustomerDetails = new List<CustomerDetailList>(),
					IsCustomerExists = false
				};

			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_SearchCustomersFromFilter");
			try
			{
				objCmd.Parameters.AddWithValue("@Email", Email);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(Mobile));

				DataTable dt = objSQL.FetchDT(objCmd);

				var customerDetails = (from DataRow dr in dt.Rows
									   select new CustomerDetailList
									   {
										   Id = Convert.ToInt64(dr["Id"]),
										   FirstName = Convert.ToString(dr["FirstName"]),
										   LastName = Convert.ToString(dr["LastName"]),
										   Email = Convert.ToString(dr["Email"]),
										   Mobile = Convert.ToString(dr["Mobile"])
									   }).ToList();

				return new SearchCustomersFromFilterResponse
				{
					CustomerDetails = customerDetails,
					IsCustomerExists = customerDetails.Count > 0 ? true : false
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

		public void UpdateQRCodePath(UpdateQRCodeModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateQRCodePath");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerBookingId", model.CustomerBookingId);
				objCmd.Parameters.AddWithValue("@QRCodePath", model.QRCodePath);
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

		public async Task UpdateQRCode(long BookingId, string LogoUrl)
		{
			/* Generate QR for Booking Id */
			var QRResponse = _qRRepo.GetCompressedStaticTigerQRImage(Convert.ToString(BookingId), LogoUrl);

			var QRCodePath = await _aWSService.UploadFile(QRResponse.data);
			UpdateQRCodeModel qRCodeModel = new UpdateQRCodeModel
			{
				CustomerBookingId = BookingId,
				QRCodePath = QRCodePath
			};
			UpdateQRCodePath(qRCodeModel);
		}

		public long AddCustomer(AddCustomerRequest model, string origin)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddCustomer");

			try
			{
				objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
				objCmd.Parameters.AddWithValue("@LastName", model.LastName);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
				objCmd.Parameters.AddWithValue("@Email", model.Email);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var Error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(Error))
					throw new AppException(Error);

				if (!string.IsNullOrEmpty(model.Email))
				{
					string ResetToken = randomTokenString();
					DateTime ResetTokenExpires = DateTime.UtcNow.AddDays(1);
					objCmd = new SqlCommand("sp_SetResetTokenForForgotPwd");
					objCmd.Parameters.AddWithValue("@Email", model.Email);
					objCmd.Parameters.AddWithValue("@ResetToken", ResetToken);
					objCmd.Parameters.AddWithValue("@ResetTokenExpires", ResetTokenExpires);
					objCmd.Parameters.AddWithValue("@IsFromCustomerApp", true);
					objSQL.UpdateDB(objCmd, true);

					sendPasswordResetEmail(model.FirstName, model.LastName, model.Email, ResetToken, origin);
				}
				return Convert.ToInt64(dtCustomer.Rows[0]["CustomerId"]);
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

		public long AddPOBusinessOfficeEmployee(POBusinessOfficeEmployees model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddPOBusinessOfficeEmployee");

			try
			{
				objCmd.Parameters.AddWithValue("@BusinessOfficeEmployeeId", model.BusinessOfficeEmployeeId);
				objCmd.Parameters.AddWithValue("@BusinessOfficeId", model.BusinessOfficeId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@OfficeDuration", model.OfficeDuration);
				objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
				objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
				objCmd.Parameters.AddWithValue("@ModifyBy", model.ModifyBy);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				return Convert.ToInt64(dtCustomer.Rows[0]["OfficeEmployeeId"]);
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

        public long AddPOBusinessOfficeEmployee_v1(POBusinessOfficeEmployeeInput model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddPOBusinessOfficeEmployee_v1");

            try
            {
                if (model.CustomerId > 0)
                    objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                objCmd.Parameters.AddWithValue("@Mobile", model.Customer?.Mobile);
                objCmd.Parameters.AddWithValue("@Email", model.Customer?.Email);
                objCmd.Parameters.AddWithValue("@FirstName", model.Customer?.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.Customer?.LastName);
                if (model.CustomerVehicleId > 0)
                    objCmd.Parameters.AddWithValue("@CustomerVehicleId", model.CustomerVehicleId);
                objCmd.Parameters.AddWithValue("@NumberPlate", model.CustomerVehicle?.NumberPlate);
                objCmd.Parameters.AddWithValue("@VehicleModal", model.CustomerVehicle?.VehicleModal);
                objCmd.Parameters.AddWithValue("@VehicleTypeId", model.CustomerVehicle?.VehicleTypeId);
                objCmd.Parameters.AddWithValue("@VehicleColorId", model.CustomerVehicle?.VehicleColorId);
                objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.CustomerVehicle?.VehicleManufacturerId);
                objCmd.Parameters.AddWithValue("@StateCode", model.CustomerVehicle?.StateCode);
                objCmd.Parameters.AddWithValue("@CountryCode", model.CustomerVehicle?.CountryCode);
                objCmd.Parameters.AddWithValue("@BusinessOfficeEmployeeId", model.BusinessOfficeEmployeeId);
                objCmd.Parameters.AddWithValue("@BusinessOfficeId", model.BusinessOfficeId);
                objCmd.Parameters.AddWithValue("@OfficeDuration", model.OfficeDuration);
                objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                objCmd.Parameters.AddWithValue("@ModifyBy", model.ModifyBy);

                DataTable dtCustomer = objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtCustomer.Rows[0]["OfficeEmployeeId"]);
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

        public long AddWhiteListCustomer(WhiteListCustomers model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_AddWhiteListCustomers_v1");

			try
			{
				objCmd.Parameters.AddWithValue("@WhiteListCustomerId", model.WhiteListCustomerId);
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
				objCmd.Parameters.AddWithValue("@NumberPlate", model.NumberPlate);
				objCmd.Parameters.AddWithValue("@VehicleModal", model.VehicleModal);
				objCmd.Parameters.AddWithValue("@VehicleTypeId", model.VehicleTypeId);
				objCmd.Parameters.AddWithValue("@VehicleColorId", model.VehicleColorId);
				objCmd.Parameters.AddWithValue("@VehicleManufacturerId", model.VehicleManufacturerId);
				objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
				objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
				objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
				objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
				objCmd.Parameters.AddWithValue("@ModifyBy", model.ModifyBy);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var error = Convert.ToString(dtCustomer.Rows[0]["Error"]);
				if (!string.IsNullOrEmpty(error))
					throw new AppException(error);

				return Convert.ToInt64(dtCustomer.Rows[0]["CustomerId"]);
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

		public WhiteListCustomerDetailResponse GetWhiteListCustomerById(long WhiteListCustomerId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetWhiteListCustomerById");
			try
			{
				objCmd.Parameters.AddWithValue("@WhiteListCustomerId", WhiteListCustomerId);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

				StatesMst state = null; Countries country = null;
				var details = (from DataRow dr in dtCustomer.Rows
							   select new WhiteListCustomerDetailResponse
							   {
								   WhiteListCustomerId = Convert.ToInt32(dr["Id"]),
								   NumberPlate = Convert.ToString(dr["NumberPlate"]),
								   ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
								   VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"]),
								   VehicleManufacturer = dr["VehicleManufacturerId"] == DBNull.Value ? null : VehicleMasterData.ListManufacturer.Find(a => a.Id == Convert.ToInt64(dr["VehicleManufacturerId"])).Name,
								   VehicleColor = dr["VehicleColorId"] == DBNull.Value ? null : VehicleMasterData.ListColor.Find(a => a.Id == Convert.ToInt64(dr["VehicleColorId"])).Name,
								   VehicleState = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).Name,
								   VehicleColorId = dr["VehicleColorId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleColorId"]),
								   VehicleManufacturerId = dr["VehicleManufacturerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["VehicleManufacturerId"]),
								   VehicleTypeId = dr["VehicleTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VehicleTypeId"]),
								   StateCode = dr["VehicleStateId"] == DBNull.Value ? null : parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["VehicleStateId"]), ref state).StateCode,
								   VehicleCountry = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).Name,
								   CountryCode = dr["VehicleCountryId"] == DBNull.Value ? null : parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["VehicleCountryId"]), ref country).CountryCode,
								   IsActive = Convert.ToBoolean(dr["IsActive"])
							   }).FirstOrDefault();
				return details;
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

		public OfficeEmployeeDetailsResponse GetPOBusinessOfficeEmployeeById(long BusinessOfficeEmployeeId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetPOBusinessOfficeEmployeeById");
			try
			{
				objCmd.Parameters.AddWithValue("@BusinessOfficeEmployeeId", BusinessOfficeEmployeeId);
				DataTable dtCustomer = objSQL.FetchDT(objCmd);


				var details = (from DataRow dr in dtCustomer.Rows
							   select new OfficeEmployeeDetailsResponse
							   {
								   BusinessOfficeEmployeeId = Convert.ToInt64(dr["BusinessOfficeEmployeeId"]),
								   CustomerInfoId = Convert.ToInt64(dr["CustomerInfoId"]),
								   BusinessOfficeId = Convert.ToInt64(dr["BusinessOfficeId"]),
								   CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
								   ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
								   NumberPlate = Convert.ToString(dr["NumberPlate"]),
								   OfficeDuration = Convert.ToInt32(dr["OfficeDuration"]),
								   FirstName = Convert.ToString(dr["FirstName"]),
								   LastName = Convert.ToString(dr["LastName"]),
								   IsActive = Convert.ToBoolean(dr["IsActive"]),
								   Email = Convert.ToString(dr["Email"]),
								   Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
								   MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
							   }).FirstOrDefault();
				return details;
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
		public WhiteListCustomerListResponse GetWhiteListCustomerList(int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue, long ParkingBusinessOwnerId)

		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetWhiteListCustomerList");
			try
			{
				objCmd.Parameters.AddWithValue("@PageNo", PageNo);
				objCmd.Parameters.AddWithValue("@PageSize", PageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", SortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", SortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);


				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				if (dtCustomer.Rows.Count > 0)
				{
					var list = (from DataRow dr in dtCustomer.Rows
								select new WhiteListCustomerList
								{
									WhiteListCustomerId = Convert.ToInt64(dr["Id"]),
									NumberPlate = Convert.ToString(dr["NumberPlate"])
								}).ToList();
					return new WhiteListCustomerListResponse { WhiteListCustomers = list, Total = Convert.ToInt32(dtCustomer.Rows[0]["TotalCount"]) };
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

		public POBusinessOfficeEmployeeList GetPOBusinessOfficeEmployeeList(long ParkingBusinessOwnerId,long? BusinessOfficeId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_GetPOBusinessOfficeEmployeeList_v1");
			try
			{
				objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@BusinessOfficeId", BusinessOfficeId);
                objCmd.Parameters.AddWithValue("@PageNo", PageNo);
				objCmd.Parameters.AddWithValue("@PageSize", PageSize);
				objCmd.Parameters.AddWithValue("@SortColumn", SortColumn);
				objCmd.Parameters.AddWithValue("@SortOrder", SortOrder);
				objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);

				DataTable dtCustomer = objSQL.FetchDT(objCmd);

				if (dtCustomer.Rows.Count > 0)
				{
					var list = (from DataRow dr in dtCustomer.Rows
								select new OfficeEmployeeDetailsResponse
								{
									BusinessOfficeEmployeeId = Convert.ToInt64(dr["BusinessOfficeEmployeeId"]),
									CustomerInfoId = Convert.ToInt64(dr["CustomerInfoId"]),
									BusinessOfficeId = Convert.ToInt64(dr["BusinessOfficeId"]),
									CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"]),
									NumberPlate = Convert.ToString(dr["NumberPlate"]),
									OfficeDuration = Convert.ToInt32(dr["OfficeDuration"]),
									FirstName = Convert.ToString(dr["FirstName"]),
									LastName = Convert.ToString(dr["LastName"]),
									IsActive = Convert.ToBoolean(dr["IsActive"]),
									Email = Convert.ToString(dr["Email"]),
									Mobile = Convert.ToString(dr["Mobile"])
								}).ToList();
					return new POBusinessOfficeEmployeeList { EmployeeList = list, Total = Convert.ToInt32(dtCustomer.Rows[0]["TotalCount"]) };
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

        public bool CheckWhiteListVehicleExists(long? WhiteListCustomerId, string NumberPlate)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CheckWhiteListVehicleExists");
			try
			{
				if (WhiteListCustomerId != null && WhiteListCustomerId > 0)
					objCmd.Parameters.AddWithValue("@WhiteListCustomerId", WhiteListCustomerId);
				objCmd.Parameters.AddWithValue("@NumberPlate", NumberPlate);

				string result = objSQL.FetchXML(objCmd);

				return Convert.ToBoolean(result);
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

		public bool CheckBuisnessOfficeEmployeeExists(long? BusinessOfficeEmployeeId, long CustomerVehicleId, long BusinessOfficeId)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_CheckBuisnessOfficeEmployeeExists");
			try
			{
				if (BusinessOfficeEmployeeId != null && BusinessOfficeEmployeeId > 0)
					objCmd.Parameters.AddWithValue("@BusinessOfficeEmployeeId", BusinessOfficeEmployeeId);
				objCmd.Parameters.AddWithValue("@CustomerVehicleId", CustomerVehicleId);
				objCmd.Parameters.AddWithValue("@BusinessOfficeId", BusinessOfficeId);

				string result = objSQL.FetchXML(objCmd);

				return Convert.ToBoolean(result);
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

		public void UpdateOfficeEmployeePayment(OfficeEmployeeListModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateOfficeEmployeePayment");
			try
			{
				objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
				objCmd.Parameters.AddWithValue("@OfficeEmployeePaymentRef", MapDataTable.ToDataTable(model.OfficeEmployeePaymentList));
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
		}

		public void UpdateGuestMobile(UpdateGuestMobileModel model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_UpdateGuestMobile");
			try
			{
				objCmd.Parameters.AddWithValue("@CustomerInfoId", model.CustomerInfoId);
				objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
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
		}
		
		public void MakeAdditionPaymentFromQRScan(AdditionalPaymentFromQRRequest model, object PaymentInfo = null)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_MakeAdditionalPaymentFromQRScan");
			try
			{

				foreach (var item in model.BookingDetails)
				{
					model.PaymentNotes += (item.StartDate + item.StartTime).ToString("MMM dd h:mm tt") + " to " + (item.EndDate + item.EndTime).ToString("MMM dd h:mm tt") + ",";
				}

				model.PaymentNotes = string.IsNullOrEmpty(model.PaymentNotes) ? model.PaymentNotes : model.PaymentNotes.TrimEnd(',');

				objCmd.Parameters.AddWithValue("@BookingId", model.BookingId);
				objCmd.Parameters.AddWithValue("@UnpaidAmount", model.UnpaidAmount);
				objCmd.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
				objCmd.Parameters.AddWithValue("@Currency", model.Currency);
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
				objCmd.Parameters.AddWithValue("@PaymentNotes", model.PaymentNotes);
				objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
				objCmd.Parameters.AddWithValue("@ConvenienceFee", model.ConvenienceFee);
				objCmd.Parameters.AddWithValue("@TotalCharges", model.TotalCharges);
				objCmd.Parameters.AddWithValue("@TaxAmount", model.TaxAmount);
				objCmd.Parameters.AddWithValue("@BookingAmount", model.BookingAmount);	
				objCmd.Parameters.AddWithValue("@BookingDetailRef", MapDataTable.ToDataTable(model.BookingDetails));
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
		}
		
		public FetchCustomerFromEmailAndMobileResponse FetchCustomerFromEmailAndMobile(FetchCustomerFromEmailAndMobileRequest model)
		{
			SQLManager objSQL = new SQLManager(_configuration);
			SqlCommand objCmd = new SqlCommand("sp_FetchCustomerFromEmailOrPhone");
            try
            {
                var passwordHash = BC.HashPassword(model.Email);
                var verificationToken = randomTokenString();

                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                objCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                objCmd.Parameters.AddWithValue("@VerificationToken", verificationToken);

                DataSet ds = objSQL.FetchDB(objCmd);
				if (ds.Tables[0].Rows.Count > 0)
				{
					var CustomerId = Convert.ToInt64(ds.Tables[0].Rows[0]["CustomerId"]);
                    var IsNewUser = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsNewUser"]);

                    var vehicleList = (from DataRow dr in ds.Tables[1].Rows
                                          select new VehicleDetails
                                          {
                                              Id = dr["Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["Id"]),
                                              NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                              VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"])
                                          }).ToList();

					if (IsNewUser)
					{
                        string redirectURL = _appSettings.ApiDomain;
                        redirectURL = redirectURL + "/verify-email?Token=" + verificationToken;
                        string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\RegistrationEmailForVerification.html");
                        MailText = string.Format(MailText, "User", "", _appSettings.AppName, redirectURL, model.Email);
						_emailService.Send(
							to: model.Email,
							subject: $"Welcome to {_appSettings.AppName}",
							html: $@"{MailText}");
                    }

					return new FetchCustomerFromEmailAndMobileResponse
					{
						CustomerId = CustomerId,
						CustomerVehicles = vehicleList.Where(e => e.NumberPlate.ToLower() != "unknown").ToList()
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

        public BookingResponse GetBookingsByVehicleNumber(string sortColumn, string sortOrder, int? pageNo, int? pageSize, long? LocationId, string? SearchValue, string? StartDate, string? EndDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingsByLocationAndVehicleNumber");
            try
            {
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
                objCmd.Parameters.AddWithValue("@LocationId", LocationId);
				objCmd.Parameters.AddWithValue("@StartDate", StartDate);
				objCmd.Parameters.AddWithValue("@EndDate", EndDate);

                DataTable dtbooking = objSQL.FetchDT(objCmd);
                if (dtbooking.Rows.Count > 0)
                {
                    var lstBooking = (from DataRow dr in dtbooking.Rows
                                      select new UpcomingBookingsList
                                      {
                                          Id = Convert.ToInt64(dr["Id"]),
										  LocationId = Convert.ToInt64(dr["LocationId"]),
                                          CustomerName = Convert.ToString(dr["CustomerName"]),
                                          Address = Convert.ToString(dr["Address"]),
                                          ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                          Mobile = Convert.ToString(dr["Mobile"]),
                                          Email = Convert.ToString(dr["Email"]),
                                          NetAmount = dr["NetAmount"] != DBNull.Value ? Convert.ToDecimal(dr["NetAmount"]) : (Decimal?)null,
                                          ExtraCharges = dr["ExtraCharges"] != DBNull.Value ? Convert.ToDecimal(dr["ExtraCharges"]) : (Decimal?)null,
                                          BookingAmount = dr["BookingAmount"] != DBNull.Value ? Convert.ToDecimal(dr["BookingAmount"]) : (Decimal?)null,
                                          StartDate = dr["StartDate"] != DBNull.Value ? Convert.ToDateTime(dr["StartDate"]) : (DateTime?)null,
                                          EndDate = dr["EndDate"] != DBNull.Value ? Convert.ToDateTime(dr["EndDate"]) : (DateTime?)null,
                                          Duration = dr["Duration"] != DBNull.Value ? Convert.ToDecimal(dr["Duration"]) : (Decimal?)null,
                                          NumberPlate = dr["NumberPlate"] == DBNull.Value ? null : Convert.ToString(dr["NumberPlate"]),
                                          VehicleModal = dr["VehicleModal"] == DBNull.Value ? null : Convert.ToString(dr["VehicleModal"])              
                                      }).ToList();

                    return new BookingResponse { BookingList = lstBooking, Total = Convert.ToInt32(dtbooking.Rows[0]["TotalCount"]) };
                }
                return new BookingResponse { BookingList = new List<UpcomingBookingsList>(), Total = 0 };
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

        public IEnumerable<ParkingLocationDto> GetParkingLocationsByOwner()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetParkingLocationsByOwner");
            try
            {
                DataTable dtLocations = objSQL.FetchDT(objCmd);
                if (dtLocations.Rows.Count > 0)
                {
                    var lstlocations = (from DataRow dr in dtLocations.Rows
                                        select new ParkingLocationDto
                                        {
                                            Id = Convert.ToInt64(dr["Id"]),
                                            Address = Convert.ToString(dr["Address"]),
                                            City = Convert.ToString(dr["City"]),
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
    }
}
