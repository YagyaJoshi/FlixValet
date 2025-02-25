using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingBLL.Repository;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.StateModels;
using ValetParkingDAL.Models.UserModels;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;
using System.Threading.Tasks;
using ValetParkingBLL.Helpers;
using System.Text.RegularExpressions;
using System.Text.Json;
using ValetParkingDAL.Models.PaymentModels.cs;
using Square.Exceptions;
using ValetParkingDAL.Enums;
using AutoMapper;

namespace ValetParkingAPI.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ValetServicesController : ControllerBase
	{
		private readonly ResourceMsgs _resourceMsgs;
		private readonly ICustomer _customerRepo;
		private readonly IStaff _staffRepo;
		private readonly IFirebase _firebaseRepo;
		public IConfiguration _configuration;
		private readonly IEmail _mailService;
		private readonly IRegion _regionRepo;
		private readonly ISMS _smsRepo;
		private readonly IParking _parkingRepo;
		private readonly ICache _cacheRepo;
		private readonly DateTimeHelper _dateTimeHelper;
		private readonly IStringLocalizer _localizer;

		private readonly ISquare _squareRepo;
		private readonly IStripe _stripeRepo;
		public readonly IPaypal _paypalRepo;
		private readonly IMapper _mapper;
		public ValetServicesController(ICustomer customerRepo, IStaff staffRepo, IFirebase firebaseRepo, IConfiguration configuration, IEmail mailService, IRegion regionRepo, ISMS smsRepo,
		IParking parkingRepo, ICache cacheRepo, DateTimeHelper dateTimeHelper, ISquare squareRepo, IStringLocalizer<Resource> localizer, IStripe stripeRepo, IPaypal paypalRepo, IMapper mapper)
		{
			_configuration = configuration;
			_mailService = mailService;
			_regionRepo = regionRepo;
			_smsRepo = smsRepo;
			_parkingRepo = parkingRepo;
			_cacheRepo = cacheRepo;
			_dateTimeHelper = dateTimeHelper;
			_customerRepo = customerRepo;
			_staffRepo = staffRepo;
			_firebaseRepo = firebaseRepo;
			_localizer = localizer;
			_squareRepo = squareRepo;
			_stripeRepo = stripeRepo;
			_paypalRepo = paypalRepo;
			_resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();
			var config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<BookingRequest, CustomerPaymentDetails>();
			});
			_mapper = config.CreateMapper();
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("AddGuestUser")]
		public Response AddGuestUser(GuestUserRequest model)
		{
			Response response = new Response();
			try
			{

				var (GuestUserId, Msg) = _customerRepo.AddGuestUser(model);
				response.Data = new CommonId { Id = GuestUserId };
				response.Status = true;
				response.Message = _localizer["OTP"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddGuestUser - ", ex.Message);

			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]

		[HttpPost("ResendOTP")]
		public Response ResendOTP(CustomerIdModel model)
		{
			Response response = new Response();
			try
			{
				var (Id, OTP) = _customerRepo.ResendOTP(model);
				response.Data = new SetGuestOTPResponse { Id = Id, OTP = OTP };
				response.Status = true;
				response.Message = _localizer["ResendOTP"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("ResendOTP - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("VerifyOTP")]
		public Response VerifyOTP(VerifyOTPRequest model)
		{
			Response response = new Response();
			try
			{
				long GId = _customerRepo.VerifyOTP(model);

				if (GId > 0)
				{
					response.Data = new CommonId { Id = GId };
					response.Status = true;
					response.Message = _localizer["VerifyOtp"];
				}
				else throw new AppException(_localizer["VerifyOtpFailed"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("VerifyOTP - ", ex.Message);
			}
			return response;
		}
		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("VerifyGuestVehicle")]
		public Response VerifyGuestVehicle(long CustomerId, string NumberPlate)
		{
			Response response = new Response();
			try
			{
				long GId = _customerRepo.VerifyGuestVehicle(CustomerId, NumberPlate);

				if (GId != 0)
				{
					response.Data = new CommonId { Id = GId };
					response.Status = false;
					response.Message = _localizer["VehicleAlreadyRegistered"];
				}
				else
				{
					response.Data = null;
					response.Status = true;
					response.Message = _localizer["RequestSuccessful"];
				}

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("VerifyGuestVehicle - ", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("CheckIn")]
		public Response CheckIn(StaffCheckinOut model)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.CheckIn(model);
				response.Status = true;
				response.Message = _localizer["CheckIn"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("CheckIn- ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("CheckOut")]
		public Response CheckOut(StaffCheckOutRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.CheckOut(model);
				response.Status = true;
				response.Message = _localizer["CheckOut"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("CheckOut- ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetCheckInDetails")]
		public Response GetCheckInDetails(long Id)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.GetCheckInDetails(Id);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetCheckInDetails- ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetUpcomingBookings")]
		public Response GetUpcomingBookings(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate)

		{
			Response response = new Response();
			try
			{
				var UpcomingBookings = _customerRepo.GetUpcomingBookings(sortColumn, sortOrder, pageNo, pageSize, LocationsList, SearchValue, SearchDate);
				response.Data = UpcomingBookings;
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
				if (UpcomingBookings.Total == 0)
				{
					response.Message = _localizer["NoUpcomingBookings"];
				}

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetUpcomingBookings", ex.Message);
			}
			return response;
		}



		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("GetAvailableParkingDetailsv1")]
		public Response GetAvailableParkingDetailsv1(GuestPreBookingRequest model)
		{
			Response response = new Response();
			try
			{
				var timeZones = _cacheRepo.CachedTimeZones();
				model.TimeZone = timeZones.Where(e => e.TimeZoneId == model.TimeZoneId).Select(a => a.Name).FirstOrDefault();
				var locResponse = _parkingRepo.GetAvailableParkingDetails(model);

				locResponse.BookingMessage = String.Format(model.BookingType.ToLower().Equals("monthly") ? _localizer["MonthlyBookingMessage"] : _localizer["BookingMessage"], locResponse.StartDate.ToString("MMMM dd"), locResponse.EndDate.ToString("MMMM dd"), (locResponse.StartDate + TimeSpan.Parse(locResponse.StartTime)).ToString("hh:mm tt"), (locResponse.StartDate + TimeSpan.Parse(locResponse.EndTime)).ToString("hh:mm tt"), locResponse.MaxStay);
				response.Data = locResponse;
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}

			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetAvailableParkingDetails", ex.Message);

			}
			return response;
		}

		//[Authorize(Roles = "ParkingManager,Valet")]
		[AllowAnonymous]
		[HttpPost("CustomerEnterToLocation")]
		public Response CustomerEnterToLocation(CustomerEnterRequest model)
		{
			Response response = new Response();
			EnterRequestDetails CustomerDetails = null;
			try
			{
				CustomerDetails = _customerRepo.CustomerEnterToLocation(model);
				if (model.GateSettingId.HasValue)
				{
					long gateSettingId = model.GateSettingId.Value;
					_parkingRepo.OpenCloseGate(gateSettingId);
				}

				if (!string.IsNullOrEmpty(CustomerDetails.Mobile))
				{
					CustomerBookingDetailsResponse detailsResponse = _customerRepo.GetBookingDetailsByCustomer(model.CustomerBookingId);
					string Msg = String.Format(_localizer["CustomerEnterMessage"], CustomerDetails.BookingTypeId == 1 ? "Regular Transit" : "Monthly Mode",
					 detailsResponse.LocationName,
					 _dateTimeHelper.GetDateFormatBasedonCurrentDate((DateTime.Parse(model.EntryDate) + TimeSpan.Parse(model.EnterTime)), DateTime.Parse(model.EntryDate)),
					  _dateTimeHelper.GetDateFormatBasedonCurrentDate(CustomerDetails.BookingTypeId == 1 ? (detailsResponse.EndDate + TimeSpan.Parse(detailsResponse.EndTime)) : (detailsResponse.StartDate + TimeSpan.Parse(detailsResponse.EndTime)), DateTime.Parse(model.EntryDate), false),
					  detailsResponse.MaxStay, CustomerDetails.Amount > 0.00m ? $" You have paid {CustomerDetails.Currency}{CustomerDetails.Amount}" : $" Make payment by clicking on the link - {_localizer["BaseUrl"]}/ElectronicPayment?BookingId={model.CustomerBookingId}");

					if (detailsResponse.BookingCategoryId == ((int)EBookingCategories.PrePaid) || detailsResponse.BookingCategoryId == ((int)EBookingCategories.PostPaid))
						_smsRepo.SendSMS(Msg, CustomerDetails.Mobile, true);
				}
				 
				response.Data = new { Id = CustomerDetails.EnterId, SendeTicket = CustomerDetails.SendeTicket };
				response.Status = true;
				response.Message = _localizer["EnterToLocation"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				if (ex is Twilio.Exceptions.ApiException && CustomerDetails != null)
				{
					response.Data = new { Id = CustomerDetails.EnterId, SendeTicket = CustomerDetails.SendeTicket };
					response.Status = true;
					response.Message = _localizer["EnterToLocation"] + ex.Message;
				}
				else
				{
					response.Data = null;
					response.Status = false;
					response.Message = ex.Message;
					_mailService.WDLogError(" CustomerEnterToLocation- ", ex.Message);
				}
			}
			return response;
		}

		//[Authorize(Roles = "ParkingManager,Valet")]
		[AllowAnonymous]
		[HttpPost("CustomerExitFromLocation")]
		public Response CustomerExitFromLocation(CustomerExitRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.CustomerExitFromLocation(model) };
				response.Status = true;
				response.Message = _localizer["ExitFromLocation"];
				if (model.GateSettingId.HasValue)
				{
					long gateSettingId = model.GateSettingId.Value;
					_parkingRepo.OpenCloseGate(gateSettingId);
				}
			}

			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("CustomerExitFromLocation - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetCustomerIdByVehicleNumber")]
		public Response GetCustomerIdByVehicleNumber(string VehicleNumber)
		{
			Response response = new Response();
			try
			{
				if (string.IsNullOrEmpty(VehicleNumber))
					throw new AppException(_localizer["VehicleNumberRequired"]);
				response.Data = _customerRepo.GetCustomerIdByVehicleNumber(VehicleNumber);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetCustomerIdByVehicleNumber - " + VehicleNumber, ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetBookingIdByVehicleNumberOld")]
		public Response GetBookingIdByVehicleNumber(string ParkingLocationId,
	   DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit)
		{
			Response response = new Response();
			try
			{
				long BookingId = 0;
				if (string.IsNullOrEmpty(VehicleNumber) && string.IsNullOrEmpty(CustomerName))
					throw new AppException(_localizer["AtleastOneFieldRequired"]);

				var bookingList = _customerRepo.GetBookingIdByVehicleNumber(ParkingLocationId, CurrentDate, VehicleNumber, CustomerName, IsExit);
				//  bookingList = bookingList.Where(a => string.IsNullOrEmpty(a.ExitDate)).ToList();



				if (bookingList != null && bookingList.Count > 0)
				{
					response.Message = _localizer["RequestSuccessful"];
					if (IsExit)
					{
						var bookrespError = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookrespError == null)
						{
							var book = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();
							if (book == null)
								throw new AppException(_localizer["VehicleAlreadyExited"]);
							else
								throw new AppException(_localizer["IsExitBookingFailed"]);
						}
						else
							BookingId = bookrespError.BookingId;
					}
					else
					{
						var bookrespError = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookrespError == null)
						{

							var FirstPriorityBooking = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).Select(a => a).FirstOrDefault();

							if (FirstPriorityBooking == null)
							{
								var AlreadyBookedData = bookingList.Where(a => (a.BookingTypeId.Equals(2) &&
								 DateTime.Parse(a.EntryDate) >= CurrentDate.Date || DateTime.Parse(a.ExitDate) >= CurrentDate.Date) || a.BookingTypeId.Equals(1)).Select(a => a).FirstOrDefault();

								if (AlreadyBookedData != null)
								{
									throw new AppException(string.Format(_localizer["AlreadyParkedandExited"], _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.EntryDate) + TimeSpan.Parse(AlreadyBookedData.EnterTime), CurrentDate), _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.ExitDate) + TimeSpan.Parse(AlreadyBookedData.ExitTime), CurrentDate)));
								}

								else
									BookingId = bookingList.FirstOrDefault(a => a.BookingTypeId == 2).BookingId;

							}
							else BookingId = FirstPriorityBooking.BookingId;




						}
						else
							throw new AppException(_localizer["IsEnterBookingFailed"]);

					}
					response.Data = new BookingIdResponse { BookingId = BookingId };
					response.Status = true;

				}
				else throw new AppException(_localizer["NoBooking"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetBookingIdByVehicleNumber", ex.Message);
			}
			return response;
		}

		//[Authorize(Roles = "ParkingManager,Valet")]
		[AllowAnonymous]
		[HttpGet("GetBookingIdByVehicleNumber")]
		public Response GetBookingIdByVehicleNumberV1(string ParkingLocationId,
	   DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit)
		{
			Response response = new Response();
			try
			{
				long BookingId = 0;
				if (string.IsNullOrEmpty(VehicleNumber) && string.IsNullOrEmpty(CustomerName))
					throw new AppException(_localizer["AtleastOneFieldRequired"]);

				var tuple = _customerRepo.GetBookingIdByVehicleNumberV1(ParkingLocationId, CurrentDate, VehicleNumber, CustomerName, IsExit);
				//  bookingList = bookingList.Where(a => string.IsNullOrEmpty(a.ExitDate)).ToList();
				var bookingList = tuple.Item1;
				var WhiteList = tuple.Item2;
				var ChargeBack = tuple.Item3;

				var bookingResposne = new BookingIdResponse();

				if (WhiteList != null)
				{
					bookingResposne.WhiteListCustomerId = WhiteList.WhiteListCustomerId;
					// response.Data = new BookingIdResponse { WhiteListCustomerId = WhiteList.WhiteListCustomerId };
					// response.Status = true;
				}
				if (ChargeBack != null)
				{
					bookingResposne.ChargeBackCustomerDetails = ChargeBack;
					// response.Data = new BookingIdResponse { ChargeBackCustomerId = ChargeBack.BusinessOfficeEmployeeId };
					// response.Status = true;
				}
				if (bookingList != null && bookingList.Count > 0)
				{
					response.Message = _localizer["RequestSuccessful"];
					if (IsExit)
					{
						var bookrespError = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookrespError == null)
						{
							var book = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();
							if (book == null)
								throw new AppException(_localizer["VehicleAlreadyExited"]);
							else
								throw new AppException(_localizer["IsExitBookingFailed"]);
						}
						else
							BookingId = bookrespError.BookingId;
					}
					else
					{
						var bookrespError = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookrespError == null)
						{

							var FirstPriorityBooking = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).Select(a => a).FirstOrDefault();

							if (FirstPriorityBooking == null)
							{
								var AlreadyBookedData = bookingList.Where(a => (a.BookingTypeId.Equals(2) &&
								 DateTime.Parse(a.EntryDate) >= CurrentDate.Date || DateTime.Parse(a.ExitDate) >= CurrentDate.Date) || a.BookingTypeId.Equals(1)).Select(a => a).FirstOrDefault();

								if (AlreadyBookedData != null)
								{
									// var Message = (string.Format(_localizer["AlreadyParkedandExited"], _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.EntryDate) + TimeSpan.Parse(AlreadyBookedData.EnterTime), CurrentDate), _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.ExitDate) + TimeSpan.Parse(AlreadyBookedData.ExitTime), CurrentDate)));
									if (WhiteList != null)
									{
										return new Response
										{
											Status = true,
											Data = bookingResposne,
											Message = "WhiteList customer"
										};
									}
									else
									{
										//throw new AppException(Message);
										var booking = bookingList.Where(e => e.BookingStartDateTime <= CurrentDate && e.BookingEndDateTime >= CurrentDate).FirstOrDefault();
										if (booking != null)
											BookingId = bookingList.FirstOrDefault().BookingId;
										

									}
								}

								else
									BookingId = bookingList.FirstOrDefault(a => a.BookingTypeId == 2).BookingId;

							}
							else BookingId = FirstPriorityBooking.BookingId;




						}
						else
							throw new AppException(_localizer["IsEnterBookingFailed"]);

					}
					bookingResposne.BookingId = BookingId;
					// response.Data = new BookingIdResponse { BookingId = BookingId };
					// response.Status = true;

				}
				response.Data = bookingResposne;
				response.Status = true;
				response.Message = bookingResposne.BookingId == null || bookingResposne.BookingId == 0 ? _localizer["NoBooking"] : "";
				// else throw new AppException(_localizer["NoBooking"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetBookingIdByVehicleNumber", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("MakeAdditionalPayment")]
		public Response MakeAdditionalPayment(AdditionalPaymentRequest model)
		{
			Response response = new Response();
			try
			{

				string paymentUrl;

				paymentUrl = $"{_localizer["BaseUrl"]}/ElectronicPayment?BookingId={model.BookingId}";

				bool IsSuccess = _customerRepo.MakeAdditionalPayment(model, paymentUrl);
				response.Status = true;

				if (model.PaymentMode.ToLower().Equals("electronic"))
				{
					response.Message = _localizer["ElectronicLinkSuccess"];
				}
				else
				{
					response.Message = _localizer["PaymentSuccessful"];
				}
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("MakeAdditionalPayment - ", ex.Message);
			}
			return response;
		}

		// [HttpGet("CalculateExtraCharges")]
		// public Response CalculateExtraCharges(long BookingId, string CurrentDate, string EnterTime, string ExitTime)
		// {
		//     Response response = new Response();
		//     try
		//     {
		//         response.Data = _customerRepo.CalculateExtraCharges(BookingId, CurrentDate, EnterTime, ExitTime);
		//         response.Status = true;
		//         response.Message = _localizer["RequestSuccessful"];
		//     }
		//     catch (AppException ex)
		//     {
		//         response.Data = null;
		//         response.Status = false;
		//         response.Message = ex.Message;
		//     }
		//     catch (Exception ex)
		//     {
		//         response.Data = null;
		//         response.Status = false;
		//         response.Message = ex.Message;
		//         _mailService.WDLogError("CalculateExtraCharges", ex.Message);
		//     }
		//     return response;
		// }

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetParkedVehicles")]
		public Response GetParkedVehicles(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate)
		{
			Response response = new Response();
			try
			{

				var parkedVehicles = _customerRepo.GetParkedVehicles(sortColumn, sortOrder, pageNo, pageSize, LocationsList, SearchValue, SearchDate);
				response.Data = parkedVehicles;
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
				if (parkedVehicles.Total == 0)
				{
					response.Message = _localizer["NoVehicleParked"];
				}

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetParkedVehicles", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("UpdateNotesforParkingSlot")]
		public Response UpdateNotesforParkingSlot(UpdateNotesRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.UpdateNotesforParkingSlot(model) };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateNotesforParkingSlot - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("AddDamageVehicle")]
		public Response AddDamageVehicle(VehicleDamage model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.AddDamageVehicle(model) };
				response.Status = true;
				response.Message = model.Id > 0 ? _localizer["UpdateDamageVehicle"] : _localizer["AddDamageVehicle"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddDamageVehicle - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("InspectUserCheckIn")]
		public Response InspectUserCheckIn(long UserId)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.InspectUserCheckIn(UserId);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetCheckInDetails- ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetNotificationListForLocation")]
		public Response GetNotificationListForLocation(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, DateTime SearchDate)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.GetNotificationListForLocation(ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, SearchDate);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetNotificationListForLocation", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
		[HttpGet("GetStaffByParkingOwner")]
		public Response GetStaffByParkingOwner(long ParkingBusinessOwnerId)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.GetStaffByParkingOwner(ParkingBusinessOwnerId);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetStaffByParkingOwner", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetUndepositedPaymentList")]
		public Response GetUndepositedPaymentList(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (UserId == 0)
					throw new AppException("UserIdRequired");

				UnDepositedPaymentResponse paymentResponse = _staffRepo.GetUndepositedPaymentList(UserId, sortColumn, sortOrder, pageNo, pageSize, CurrentDate);

				if (paymentResponse.LastDepositedDate != null)
					paymentResponse.LastDepositedMessage = string.Format(_localizer["LastDepositedMessage"], paymentResponse.Symbol + paymentResponse.LastDepositedAmount, Convert.ToDateTime(paymentResponse.LastDepositedDate).ToString("MMM dd h:mm tt"));
				//paymentResponse.LastDepositedMessage = string.Format(_localizer["LastDepositedMessage"], paymentResponse.Symbol + paymentResponse.LastDepositedAmount, _dateTimeHelper.GetDateFormatBasedonCurrentDate(Convert.ToDateTime(paymentResponse.LastDepositedDate), DateTime.Now.Date));
				response.Data = paymentResponse;
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetUndepositedPaymentList", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("UpdateDepositedPayment")]
		public Response UpdateDepositedPayment(UpdateDepositedPayment model)
		{
			Response response = new Response();
			try
			{
				var TotalAmount = _staffRepo.UpdateDepositedPayment(model);

				if (TotalAmount == 0.00m) throw new AppException(_localizer["NoCashStatus"]);
				response.Status = true;
				response.Message = string.Format(_localizer["DepositPaymentStatus"], TotalAmount);
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateDepositedPayment", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetLocationNamesFromList")]
		public Response GetLocationNamesFromList(string Locations)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetLocationNamesFromList(Locations);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetLocationNamesFromList", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("DeleteVehicleDamageReport")]
		public Response DeleteVehicleDamageReport(VehicleDamageIdModel model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.DeleteVehicleDamageReport(model) };
				response.Status = true;
				response.Message = _localizer["DeleteSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("DeleteVehicleDamageReport", ex.Message);

			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("ChangeVehicleForBooking")]
		public Response ChangeVehicleForBooking(ChangeVehicleRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.ChangeVehicleForBooking(model) };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("ChangeVehicleForBooking- ", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetDamageVehicleReport")]
		public Response GetDamageVehicleReport(long ParkingLocationId, string CurrentDate, string SearchValue)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0)
					throw new AppException("ParkingLocationIdRequired");
				response.Data = _parkingRepo.GetDamageVehicleReport(ParkingLocationId, CurrentDate, SearchValue);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetDamageVehicleReport", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("GetCheckInReport")]
		public Response GetCheckInReport(long ParkingLocationId,
		  string CurrentDate, string SearchValue, bool HasCheckedIn)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0)
					throw new AppException("ParkingLocationIdRequired");
				response.Data = _parkingRepo.GetCheckInReport(ParkingLocationId, CurrentDate, SearchValue, HasCheckedIn);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

				_mailService.WDLogError("GetCheckInReport", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("AddGuestDetails")]
		public Response AddGuestDetails(GuestDetailsRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = _customerRepo.AddGuestDetails(model, null);
				response.Status = true;
				response.Message = _localizer["AddGuestUser"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddGuestDetails", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("SetGuestOtp")]
		public Response SetGuestOtp(SetGuestOtpRequest model)
		{
			Response response = new Response();
			try
			{
				var (Id, OTP) = _customerRepo.SetGuestOtp(model);
				response.Data = new SetGuestOTPResponse { Id = Id, OTP = OTP };
				response.Message = _localizer["OTP"];
				response.Status = true;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("SetGuestOtp", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("UpdateNotificationStatus")]
		public Response UpdateNotificationStatus(UpdateNotificationStatusModel model)
		{
			Response response = new Response();

			try
			{
				_customerRepo.UpdateNotificationStatus(model);
				response.Data = new CommonId { Id = model.NotificationId };
				response.Message = _localizer["RequestAccepted"];
				response.Status = true;

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateNotificationStatus", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpPost("SendAcknowledgement")]
		public Response SendAcknowledgement(StaffNotificationModel model)
		{
			Response response = new Response();

			try
			{

				NotificationDetails notificationDetails = _customerRepo.GetNotificationDetails(model);

				if (notificationDetails != null)
				{
					string fcmresponse = _firebaseRepo.SendNotificationtoCustomer(notificationDetails.DeviceTokens, notificationDetails.BadgeCount, "Request Acknowledgement", model.NotificationMessage);

					if (fcmresponse == null) throw new AppException(_localizer["NotificationFailed"]);
					var FbResp = JsonSerializer.Deserialize<FirebaseResponse>(fcmresponse);

					if (FbResp.success == 0) throw new AppException(_localizer["NotificationFailed"]);

					response.Data = new CommonId { Id = _customerRepo.AddNotificationbyStaff(model, notificationDetails) };
					response.Message = _localizer["AcknowledgementSent"];
					response.Status = true;
				}
				else throw new AppException(_localizer["NoRequestInitiated"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("SendAcknowledgement", ex.Message);
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("FetchGuestfromVehicle")]
		public Response FetchGuestfromVehicle(string NumberPlate)
		{
			Response response = new Response();
			try
			{

				response.Data = _customerRepo.FetchGuestfromVehicle(NumberPlate);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

				_mailService.WDLogError("FetchGuestfromVehicle", ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		// [Authorize(Roles = "ParkingManager,Valet")]
		[HttpGet("FetchGuestfromVehiclev1")]
		public Response FetchGuestfromVehiclev1(string NumberPlate)
		{
			Response response = new Response();
			try
			{

				response.Data = _customerRepo.FetchGuestfromVehiclev1(NumberPlate);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

				_mailService.WDLogError("FetchGuestfromVehicle", ex.Message);
			}
			return response;
		}


		[HttpGet("ScanReceipt")]
		public Response ScanReceipt(long CustomerBookingId, bool IsExit, DateTime CurrentDate)
		{
			Response response = new Response();
			try
			{
				var bookingList = _customerRepo.ScanReceipt(CustomerBookingId, IsExit);
				if (bookingList != null && bookingList.Count > 0)
				{
					response.Message = _localizer["RequestSuccessful"];
					if (IsExit)
					{
						var bookresp = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookresp == null)
						{
							var book = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();
							if (book == null)
								throw new AppException(_localizer["VehicleAlreadyExited"]);
							else
								throw new AppException(_localizer["IsExitBookingFailed"]);
						}
					}
					else
					{
						var bookrespError = bookingList.Where(a => !string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).FirstOrDefault();

						if (bookrespError == null)
						{

							var FirstPriorityBooking = bookingList.Where(a => string.IsNullOrEmpty(a.EntryDate) && string.IsNullOrEmpty(a.ExitDate)).Select(a => a).FirstOrDefault();

							if (FirstPriorityBooking == null)
							{
								var AlreadyBookedData = bookingList.Where(a => (a.BookingTypeId.Equals(2) &&
								   DateTime.Parse(a.EntryDate) >= CurrentDate.Date || DateTime.Parse(a.ExitDate) >= CurrentDate.Date) || a.BookingTypeId.Equals(1)).Select(a => a).FirstOrDefault();

								if (AlreadyBookedData != null)
								{
									throw new AppException(string.Format(_localizer["AlreadyParkedandExited"], _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.EntryDate) + TimeSpan.Parse(AlreadyBookedData.EnterTime), CurrentDate), _dateTimeHelper.GetDateFormatBasedonCurrentDate(DateTime.Parse(AlreadyBookedData.ExitDate) + TimeSpan.Parse(AlreadyBookedData.ExitTime), CurrentDate)));
								}

							}

						}
						else
							throw new AppException(_localizer["IsEnterBookingFailed"]);

					}
					response.Data = new BookingIdResponse { BookingId = CustomerBookingId };
					response.Status = true;
				}
				else throw new AppException(_localizer["NoBooking"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("ScanReceipt", ex.Message);
			}
			return response;
		}

		[HttpPost("UpdateGuestMobile")]
		public Response UpdateGuestMobile(UpdateGuestMobileModel model)
		{
			Response response = new Response();

			try
			{
				_customerRepo.UpdateGuestMobile(model);
				response.Message = _localizer["RequestSuccessful"];
				response.Status = true;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateGuestMobile", ex.Message);
			}
			return response;
		}


		[AllowAnonymous]
		[HttpPost("GetSquareUpReaderToken")]
		public Response GetSquareUpReaderToken(SquareUpRequest model)
		{
			Response response = new Response();

			try
			{
				dynamic res = _squareRepo.GetSquareUpReaderToken(model);
				response.Data = res.Result;
				response.Message = _localizer["RequestSuccessful"];
				response.Status = true;
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.InnerException.Message;
			}
			return response;
		}

		[AllowAnonymous]
		[HttpPost("FetchCustomerFromEmailAndMobile")]
		public Response FetchCustomerFromEmailAndMobile(FetchCustomerFromEmailAndMobileRequest model)
		{
			Response response = new Response();

			try
			{
				response.Data = _customerRepo.FetchCustomerFromEmailAndMobile(model);
				response.Message = _localizer["RequestSuccessful"];
				response.Status = true;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.InnerException.Message;
				_mailService.WDLogError("FetchCustomerFromEmailOrMobile - ", ex.Message);
			}
			return response;
		}


        [Authorize(Roles = "EF")]
        [HttpGet("GetBookingsByVehicleNumber")]
        public Response GetBookingsByVehicleNumber(string sortColumn, string sortOrder, int? pageNo, int? pageSize, long? LocationId, string? SearchValue, string? StartDate, string? EndDate)

        {
            Response response = new Response();
            try
            {
                var UpcomingBookings = _customerRepo.GetBookingsByVehicleNumber(sortColumn, sortOrder, pageNo, pageSize, LocationId, SearchValue, StartDate, EndDate);
                response.Data = UpcomingBookings;
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
                if (UpcomingBookings.Total == 0)
                {
                    response.Message = _localizer["NoBooking"];
                }

            }
            catch (AppException ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("GetBookingsByVehicleNumber", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "EF")]
        [HttpGet("GetParkingLocationsByOwner")]
        public Response GetParkingLocationsByOwner()
        {
            Response response = new Response();
            try
            {
                response.Data = _customerRepo.GetParkingLocationsByOwner();
                if (response.Data == null)
                    throw new AppException(_localizer["LocationNotFound"]);
                response.Message = _localizer["RequestSuccessful"];
                response.Status = true;
            }
            catch (AppException ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("GetBookingsByVehicleNumber", ex.Message);
            }
            return response;
        }


    }
}