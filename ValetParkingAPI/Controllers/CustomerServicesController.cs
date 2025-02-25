using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;
using AutoMapper;
using ValetParkingDAL.Enums;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingBLL.Helpers;

namespace ValetParkingAPI.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class CustomerServicesController : ControllerBase
	{
		private readonly ResourceMsgs _resourceMsgs;
		private readonly ICustomer _customerRepo;
		private readonly IRegion _regionRepo;
		private readonly IParking _parkingRepo;
		public IConfiguration _configuration;
		private readonly ICache _cacheRepo;
		private readonly IEmail _mailService;
		private readonly IFirebase _firebaseRepo;
		private readonly IStripe _stripeRepo;
		private readonly ISquare _squareRepo;
		private readonly IPaypal _paypalRepo;
		private readonly DateTimeHelper _dateTimeHelper;
		private readonly IStringLocalizer _localizer;
		private readonly IMapper _mapper;

		public CustomerServicesController(ICustomer customerRepo, IRegion regionRepo, IParking parkingRepo, IMapper mapper, IConfiguration configuration, ICache cacheRepo, IEmail mailService, IFirebase firebaseRepo, IStripe stripeRepo, ISquare squareRepo, IPaypal paypalRepo, DateTimeHelper dateTimeHelper, IStringLocalizer<Resource> localizer)
		{
			_configuration = configuration;
			_mailService = mailService;
			_firebaseRepo = firebaseRepo;
			_stripeRepo = stripeRepo;
			_squareRepo = squareRepo;
			_dateTimeHelper = dateTimeHelper;
			_cacheRepo = cacheRepo;
			_customerRepo = customerRepo;
			_regionRepo = regionRepo;
			_parkingRepo = parkingRepo;
			_paypalRepo = paypalRepo;
			_resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();
			_localizer = localizer;
			var config = new MapperConfiguration(cfg =>
		  {
			  cfg.CreateMap<PushNotificationModel, Notification>();
		  });
			_mapper = config.CreateMapper();

		}

		[Authorize(Roles = "Customer")]
		[HttpPost("AddCustomerInfo")]
		public Response AddCustomerInfo(CustomerInfo model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.AddCustomerInfo(model) };
				response.Status = true;
				response.Message = model.Id > 0 ? _localizer["CustomerUpdated"] : _localizer["CustomerAdded"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddCustomerInfo - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetCustomerInfo")]
		public Response GetCustomerInfoById(long CustomerId)
		{
			Response response = new Response();
			try
			{

				response.Data = _customerRepo.GetCustomerInfoById(CustomerId);
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
				_mailService.WDLogError("GetCustomerInfoById - " + CustomerId, ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpPost("SearchParkingLocations")]
		public Response SearchParkingLocations(CurrentLocationRequest model)
		{
			Response response = new Response();
			try
			{
				TimeSpan StartTime = TimeSpan.Parse(model.StartTime), EndTime = TimeSpan.Parse(model.EndTime);

				if ((DateTime.Parse(model.StartDate.ToShortDateString()) + StartTime) >= (DateTime.Parse(model.EndDate.ToShortDateString()) + EndTime))
					throw new AppException(_localizer["EnterAndExitDateTime"]);

				TimeSpan tsDiff = (model.EndDate - model.StartDate);
				if (model.BookingType.ToLower() == "monthly" && tsDiff.Days != 29)
					throw new AppException(_localizer["MonthlyBooking30DayDiffError"]);

				var timeZones = _cacheRepo.CachedTimeZones();
				model.TimeZone = timeZones.Where(e => e.TimeZoneId == model.TimeZoneId).Select(a => a.Name).FirstOrDefault();

				response.Data = _parkingRepo.SearchParkingLocations(model);
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
				_mailService.WDLogError("SearchParkingLocations - ", ex.Message);

			}
			return response;
		}

		[Authorize]
		[HttpPost("AddCustomerVehicle")]
		public Response AddCustomerVehicles(CustomerVehicles model)
		{
			Response response = new Response();
			try
			{
				if (model.CustomerInfoId <= 0)
					throw new AppException("CustomerId is required");
				response.Data = new CommonId { Id = _customerRepo.AddCustomerVehicles(model) };
				response.Status = true;
				response.Message = model.Id > 0 ? _localizer["VehicleUpdated"] : _localizer["VehicleAdded"];
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
				_mailService.WDLogError("AddCustomerVehicle - ", ex.Message);
			}
			return response;
		}
		[Authorize(Roles = "Valet,ParkingManager,Customer,ParkingSupervisor,BusinessOffice")]
		[HttpGet("GetCustomerVehicles")]
		public Response GetCustomerVehicles(long CustomerInfoId)
		{
			Response response = new Response();
			try

			{
				if (CustomerInfoId.Equals(0))
					throw new AppException(_localizer["CustomerIdRequired"]);
				response.Data = _customerRepo.GetCustomerVehicles(CustomerInfoId);
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
				_mailService.WDLogError("GetCustomerVehicles - " + CustomerInfoId, ex.Message);
			}
			return response;
		}
		[Authorize(Roles = "Valet,ParkingManager,Customer")]
		[HttpGet("GetVehicleInfoById")]
		public Response GetVehicleInfoById(long Id)
		{
			Response response = new Response();
			try
			{
				if (Id.Equals(0))
					throw new AppException(_localizer["VehicleIdRequired"]);
				response.Data = _customerRepo.GetVehicleInfoById(Id);
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
				_mailService.WDLogError("GetVehicleInfoById - " + Id, ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("UpdateNotificationMode")]
		public Response UpdateNotificationMode(UpdateNotificationRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.UpdateNotificationMode(model) };
				response.Status = true;
				response.Message = _localizer["NotificationModeUpdate"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateNotificationMode - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("AddNotification")]
		public Response AddNotification(Notification model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.AddNotification(model) };
				response.Status = true;
				response.Message = _localizer["NotificationAdded"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddNotification - " + model, ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetNotifications")]
		public Response GetNotifications(long CustomerInfoId, long ParkingLocationId)
		{
			Response response = new Response();
			try
			{
				if (CustomerInfoId.Equals(0) || ParkingLocationId.Equals(0))
					throw new AppException(_localizer["CustomerAndLocationIdRequired"]);
				response.Data = _customerRepo.GetNotifications(CustomerInfoId, ParkingLocationId);
				if (response.Data == null)
					throw new AppException(_localizer["NotificationsNotFound"]);
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
				_mailService.WDLogError("GetNotifications - " + CustomerInfoId + ParkingLocationId, ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		// [Authorize(Roles = "Customer")]
		[HttpGet("GetEstimatedBookingDetailsByLoc")]
		public Response GetEstimatedBookingDetailsByLoc(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, string BookingType, bool IsFullTimeBooking, long? CustomerVehicleId, string TimeZoneId, bool IsFromQRScan = false)
		{
			Response response = new Response();
			try
			{
				DateTime datetime;
				bool checkStartTime = DateTime.TryParse(StartTime, out datetime);
				bool checkEndTime = DateTime.TryParse(EndTime, out datetime);
				if (!checkStartTime)
					throw new AppException(_localizer["InvalidStartTime"]);
				if (!checkEndTime)
					throw new AppException(_localizer["InvalidEndTime"]);

				if (BookingType.ToLower().Equals("monthly"))
				{
					if (StartTime == EndTime)
						throw new AppException(_localizer["StartTimeAndEndTimeError"]);
				}
				EndDate = DateTime.Parse(EndDate.ToShortDateString());
				StartDate = DateTime.Parse(StartDate.ToShortDateString());
				TimeSpan ts = (EndDate + TimeSpan.Parse(EndTime)) - (StartDate + TimeSpan.Parse(StartTime));

				if (ts.TotalHours <= 0) throw new AppException(_localizer["DateRangeError"]);
				else
				{

					var timeZones = _cacheRepo.CachedTimeZones();
					var TimeZone = timeZones.Where(e => e.TimeZoneId == TimeZoneId).Select(a => a.Name).FirstOrDefault();
					ParkingLocDetailsResponse locResponse = _parkingRepo.GetEstimatedBookingDetailsByLoc(ParkingLocationId, CustomerId, StartDate, EndDate, StartTime, EndTime, BookingType, IsFullTimeBooking, CustomerVehicleId, TimeZone, IsFromQRScan);
					response.Data = locResponse;



					if (response.Data == null)
						throw new AppException(_localizer["LocationDetailsNotFound"]);

					else
					{
						locResponse.BookingMessage = String.Format(BookingType.ToLower().Equals("monthly") ? _localizer["MonthlyBookingMessage"] : _localizer["BookingMessage"], locResponse.StartDate.ToString("MMM dd"), locResponse.EndDate.ToString("MMM dd"), (locResponse.StartDate + TimeSpan.Parse(locResponse.StartTime)).ToString("hh:mm tt"), (locResponse.StartDate + TimeSpan.Parse(locResponse.EndTime)).ToString("hh:mm tt"), locResponse.MaxStay);

					}
					response.Status = true;
					response.Message = _localizer["RequestSuccessful"];
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
				_mailService.WDLogError("GetEstimatedBookingDetailsByLoc", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("AddCustomerAddress")]
		public Response AddCustomerAddresses(CustomerAddress model)
		{
			Response response = new Response();
			try
			{
				//Address added successfully
				response.Data = new CommonId { Id = _customerRepo.AddCustomerAddress(model) };
				response.Status = true;
				response.Message = _localizer["CustomerAddressUpdate"];
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
				_mailService.WDLogError("AddCustomerAddress- ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetCustomerAddressById")]
		public Response GetCustomerAddressById(long CustomerId)
		{
			Response response = new Response();
			try
			{
				if (CustomerId.Equals(0))
					throw new AppException(_localizer["CustomerIdRequired"]);
				response.Data = _customerRepo.GetCustomerAddressById(CustomerId);
				if (response.Data == null)
					throw new AppException(_localizer["CustomerNotFound"]);
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
				_mailService.WDLogError("GetCustomerAddressById - " + CustomerId, ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("CancelBooking")]
		public Response CancelBooking(CancelBookingRequest model)
		{
			Response response = new Response();
			try
			{
				var bookresp = _customerRepo.GetBookingDetailsById(model.BookingId);
				response.Data = _customerRepo.CancelBooking(model);
				response.Status = true;
				if (bookresp != null)
				{
					response.Message = string.Format(_localizer["CancelBooking"], bookresp.NumberPlate, _dateTimeHelper.GetDateFormatBasedonCurrentDate(bookresp.StartDate + TimeSpan.Parse(bookresp.StartTime), model.CurrentDate), _dateTimeHelper.GetDateFormatBasedonCurrentDate(bookresp.EndDate + TimeSpan.Parse(bookresp.EndTime), model.CurrentDate));
				}
				else
					throw new AppException(_localizer["NoBooking"]);
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
				_mailService.WDLogError("CancelBooking - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetBookingDetailsById")]
		public Response GetBookingDetailsById(long Id)
		{
			Response response = new Response();
			try
			{
				if (Id.Equals(0))
					throw new AppException(_localizer["BookingIdRequired"]);
				response.Data = _customerRepo.GetBookingDetailsById(Id);
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
				_mailService.WDLogError("GetBookingDetailsById - " + Id, ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("UpdateGuestUserDetails")]
		public Response UpdateGuestUserDetails(GuestUserDetailRequest model)

		{
			Response response = new Response();
			try
			{

				response.Data = new CommonId { Id = _customerRepo.UpdateGuestUserDetails(model) };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateGuestUserDetails - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetCustomerBookingList")]
		public Response GetCustomerBookingList(string sortColumn, string sortOrder, int pageNo, int? pageSize, long CustomerId, string BookingType, DateTime SearchDate, string SearchMode)


		{
			Response response = new Response();
			try
			{
				if (BookingType.Equals("hourly") && string.IsNullOrEmpty(SearchMode))
					throw new AppException(_localizer["ModeOfSearch"]);
				response.Data = _customerRepo.GetCustomerBookingList(sortColumn, sortOrder, pageNo, pageSize, CustomerId, BookingType, SearchDate, SearchMode);
				if (response.Data == null)
					throw new AppException(_localizer["NoBooking"]);
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
				_mailService.WDLogError("GetCustomerBookingList", ex.Message);

			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("EditCustomerInfo")]
		public Response EditCustomerInfo(EditCustomerInfoRequest model)
		{
			Response response = new Response();
			try
			{
				_customerRepo.EditCustomerInfo(model);
				response.Status = true;
				response.Message = _localizer["UpdateProfile"];
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
				_mailService.WDLogError("EditCustomerInfo", ex.Message);

			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpPost("DeleteCustomerVehicle")]
		public Response DeleteCustomerVehicle(CustomerVehicleIdmodel model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.DeleteCustomerVehicle(model) };
				response.Status = true;
				response.Message = _localizer["VehicleDeleteSuccessful"];
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
				_mailService.WDLogError("DeleteCustomerVehicle", ex.Message);

			}
			return response;
		}

		// [HttpPost("SendVehiclePushNotifications")]
		// public Response SendVehiclePushNotifications(PushNotificationModel model)
		// {
		//   }

		[Authorize(Roles = "Customer")]
		[HttpPost("RequestVehicle")]
		public Response RequestVehicle(RequestVehicleModel model)
		{
			Response response = new Response();
			try
			{
				if (model.CustomerId.Equals(0))
					throw new AppException(_localizer["CustomerIdRequired"]);

				var pushNotificationModel = _customerRepo.RequestVehicle(model);

				if (pushNotificationModel != null)
				{

					string NotificationMsg = string.Format(_localizer["RequestVehicleMessage"], pushNotificationModel.CustomerName, pushNotificationModel.NumberPlate, pushNotificationModel.LocationName);

					string Fbaseresponse = _firebaseRepo.SendNotificationtoStaff(pushNotificationModel.ParkingLocationId, _localizer["RequestVehicleTitle"] + " - " + pushNotificationModel.NumberPlate, NotificationMsg, model.NotificationDateTime);

					// if (Fbaseresponse == null) throw new AppException(_localizer["NotificationDisabled"]);
					// var FbResp = JsonSerializer.Deserialize<FirebaseResponse>(Fbaseresponse);

					// if (FbResp.success > 0)
					// {
					Notification notification = _mapper.Map<Notification>(pushNotificationModel);
					notification.NotificationType = ENotificationType.RequestVehicle.ToString();
					notification.Message = NotificationMsg;
					_customerRepo.AddNotification(notification);

					response.Data = pushNotificationModel;
					response.Status = true;
					response.Message = _localizer["NotificationSent"];
					// }
					// else throw new AppException(_localizer["NotificationFailed"]);
				}
				else throw new AppException(_localizer["NoVehicleIsParked"]);
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
				_mailService.WDLogError("RequestVehicle", ex.Message);

			}
			return response;
		}


		[Authorize(Roles = "Customer")]
		[HttpGet("GetNotificationByCustomerId")]
		public Response GetNotificationByCustomerId(long CustomerId, string sortColumn, string sortOrder, int pageNo, int? pageSize)
		{
			Response response = new Response();
			try
			{
				response.Data = _customerRepo.GetNotificationByCustomerId(CustomerId, sortColumn, sortOrder, pageNo, pageSize);
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
				_mailService.WDLogError("GetNotificationByCustomerId", ex.Message);
			}
			return response;
		}

		// [Authorize(Roles = "Customer")]
		// [HttpGet("GetBookingPaymentDetails")]
		// public Response GetBookingPaymentDetails(long CustomerBookingId, DateTime CurrentDate)
		// {
		//     Response response = new Response();
		//     try
		//     {
		//         if (CustomerBookingId.Equals(0))
		//             throw new AppException(_localizer["BookingIdRequired"]);

		//         var PaymentDetails = _customerRepo.GetBookingPaymentDetails(CustomerBookingId, CurrentDate);
		//         response.Data = PaymentDetails;


		//         if (response.Data == null)
		//             throw new AppException(_localizer["BookingNotFound"]);

		//         if (PaymentDetails.DueAmount == 0.00m)
		//             throw new AppException(_localizer["NoDues"]);
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
		//         _mailService.WDLogError("GetBookingPaymentDetails", ex.Message);
		//     }
		//     return response;
		// }

		// [AllowAnonymous]
		// [HttpGet("GetElectronicPaymentDetails")]
		// public Response GetElectronicPaymentDetails(long BookingId, DateTime CurrentDate)
		// {

		//     Response response = new Response();
		//     try
		//     {
		//         ElectronicPaymentDetails bookingDetails = _customerRepo.GetElectronicPaymentDetails(BookingId, CurrentDate);
		//         response.Data = bookingDetails;
		//         if (bookingDetails == null)
		//             throw new AppException(_localizer["BookingNotFound"]);

		//         else
		//         {
		//             bookingDetails.BookingMessage = String.Format(_localizer["BookingMessage"], bookingDetails.StartDate.ToString("MMM dd"), bookingDetails.EndDate.ToString("MMM dd"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.StartTime)).ToString("hh:mm tt"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.EndTime)).ToString("hh:mm tt"), bookingDetails.MaxStay);

		//         }
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
		//         _mailService.WDLogError("GetElectronicPaymentDetails - " + BookingId, ex.Message);
		//     }
		//     return response;

		// }

		[AllowAnonymous]
		[HttpGet("GetElectronicPaymentDetails")]
		public Response GetElectronicPaymentDetails(long BookingId)
		{

			Response response = new Response();
			try
			{
				ElectronicPaymentDetails bookingDetails = _customerRepo.GetElectronicPaymentDetails(BookingId);
				response.Data = bookingDetails;
				if (bookingDetails == null)
					throw new AppException(_localizer["BookingNotFound"]);
				// else
				// {
				//     bookingDetails.BookingMessage = String.Format(_localizer["BookingMessage"], bookingDetails.StartDate.ToString("MMM dd"), bookingDetails.EndDate.ToString("MMM dd"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.StartTime)).ToString("hh:mm tt"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.EndTime)).ToString("hh:mm tt"), bookingDetails.MaxStay);

				// }
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
				_mailService.WDLogError("GetElectronicPaymentDetails - " + BookingId, ex.Message);
			}
			return response;

		}

		[AllowAnonymous]
		[HttpPost("MakeElectronicPayment")]
		public Response MakeElectronicPayment(ElectronicPaymentRequest model)
		{
			Response response = new Response();
			try
			{

				CheckCustomerDueAmount cust = _customerRepo.CheckCustomerDueAmount(model.BookingId);
				if (model.UnpaidAmount == 0.00m || cust.UnpaidAmount == 0.00m)
					throw new AppException(_localizer["NoDues"]);

				var timeZones = _cacheRepo.CachedTimeZones();
				model.TimeZone = timeZones.Where(e => e.TimeZoneId == cust.TimeZoneId).Select(a => a.Name).FirstOrDefault();

				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
				model.CurrentDate = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

				object paymentResponse = null;
				CustomerPaymentDetails customerDetails = _customerRepo.GetCustomerPaymentDetails(model.CustomerId, model.ParkingLocationId);
				customerDetails.ZipCode = model.ZipCode ?? customerDetails.ZipCode;
				customerDetails.PaypalCustomerId = customerDetails.PaypalCustomerId ?? model.PaypalCustomerId;

				decimal Amount = model.UnpaidAmount;

				if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Stripe.ToString().ToLower()))
				{

					paymentResponse = _stripeRepo.ChargePayment(model.StripeInfo, Amount, customerDetails);
					if (paymentResponse is StripeErrorResponse)
						throw new AppException(((StripeErrorResponse)paymentResponse).error.message);

				}
				if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Square.ToString().ToLower()))
				{
					paymentResponse = _squareRepo.ChargePayment(model.SquareupInfo, Amount, customerDetails);
					if (paymentResponse is SquareupErrorResponse)
						throw new AppException(((SquareupErrorResponse)paymentResponse).errors.FirstOrDefault().detail);
				}
				if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Paypal.ToString().ToLower()))
				{
					paymentResponse = _paypalRepo.ChargePayment(model.PaypalInfo, Amount, customerDetails);
					if (paymentResponse is PaypalErrorResponse)
						throw new AppException(((PaypalErrorResponse)paymentResponse).Message);
				}

				if (paymentResponse == null)
					throw new AppException(_localizer["PaymentUnsuccessful"]);


				_customerRepo.MakeElectronicPayment(model, paymentResponse);


				response.Data = null;
				response.Status = true;
				response.Message = _localizer["PaymentSuccessful"];

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

				_mailService.WDLogError("MakeElectronicPayment", ex.Message);

			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetParkedVehicleListByCustomerId")]
		public Response GetParkedVehicleListByCustomerId(long CustomerId)
		{
			Response response = new Response();
			try
			{
				if (CustomerId == 0)
					throw new AppException(_localizer["CustomerIdRequired"]);
				response.Data = _customerRepo.GetParkedVehicleListByCustomerId(CustomerId);
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
				_mailService.WDLogError("GetParkedVehicleListByCustomerId - ", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "Customer")]
		[HttpGet("GetBookingDetailsByCustomer")]
		public Response GetBookingDetailsByCustomer(long BookingId)
		{


			Response response = new Response();
			try
			{
				CustomerBookingDetailsResponse bookingDetails = _customerRepo.GetBookingDetailsByCustomer(BookingId);
				response.Data = bookingDetails;
				if (bookingDetails == null)
					throw new AppException(_localizer["BookingNotFound"]);

				else
				{
					bookingDetails.BookingMessage = String.Format(bookingDetails.BookingTypeId == 2 ?_localizer["MonthlyBookingMessage"] : _localizer["BookingMessage"], bookingDetails.StartDate.ToString("MMM dd"), bookingDetails.EndDate.ToString("MMM dd"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.StartTime)).ToString("hh:mm tt"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.EndTime)).ToString("hh:mm tt"), bookingDetails.MaxStay);

				}
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
				_mailService.WDLogError("GetBookingDetailsByCustomer - " + BookingId, ex.Message);
			}
			return response;
		}

		[HttpPost("BrowserLaunch")]
		public Response BrowserLaunch(BrowserLaunchRequest model)
		{
			Response response = new Response();
			try
			{
				var (id, UnreadCount) = _customerRepo.BrowserLaunch(model);
				response.Data = new { Id = id, UnreadCount = UnreadCount };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("BrowserLaunch- ", ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpGet("GetPreBookingDetails")]
		public Response GetPreBookingDetails(long ParkingLocationId, long CustomerId)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId.Equals(0))
					throw new AppException(_localizer["ParkingLocationIdRequired"]);
				response.Data = _customerRepo.GetPreBookingDetails(ParkingLocationId, CustomerId);
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
				_mailService.WDLogError("GetPreBookingDetails - ", ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpGet("GetExtendBookingDetails")]
		public Response GetExtendBookingDetails(long CustomerBookingId, long CustomerId, DateTime CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (CustomerBookingId == 0)
					throw new AppException(_localizer["BookingIdRequired"]);
				response.Data = _customerRepo.GetExtendBookingDetails(CustomerBookingId, CustomerId, CurrentDate);
				if (response.Data == null)
					throw new AppException(_localizer["BookingNotFound"]);
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
				_mailService.WDLogError("GetExtendBookingDetails- ", ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpPost("GetClientToken")]
		public Response GetClientToken(PaypalModel model)
		{
			Response response = new Response();
			try
			{
				var res = _paypalRepo.GetClientToken(model);
				response.Data = new ClientTokenResponse { PaypalCustomerId = res.Item1, ClientToken = res.Item2, };
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
			}
			return response;
		}
		
		[AllowAnonymous]
        [HttpPost("MakeAdditionalPaymentFromQRScan")]
        public Response MakeAdditionalPaymentFromQRScan(AdditionalPaymentFromQRRequest model)
        {
            Response response = new Response();
            try
            {
                CheckCustomerDueAmount cust = _customerRepo.CheckCustomerDueAmount(model.BookingId);

                var timeZones = _cacheRepo.CachedTimeZones();
                model.TimeZone = timeZones.Where(e => e.TimeZoneId == cust.TimeZoneId).Select(a => a.Name).FirstOrDefault();

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
                model.CurrentDate = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

                object paymentResponse = null;
                CustomerPaymentDetails customerDetails = _customerRepo.GetCustomerPaymentDetails(model.CustomerId, model.ParkingLocationId);
                customerDetails.ZipCode = model.ZipCode ?? customerDetails.ZipCode;
                customerDetails.PaypalCustomerId = customerDetails.PaypalCustomerId ?? model.PaypalCustomerId;

                decimal Amount = model.UnpaidAmount;

                if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Stripe.ToString().ToLower()))
                {
                    paymentResponse = _stripeRepo.ChargePayment(model.StripeInfo, Amount, customerDetails);
                    if (paymentResponse is StripeErrorResponse)
                        throw new AppException(((StripeErrorResponse)paymentResponse).error.message);

                }
                if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Square.ToString().ToLower()))
                {
                    paymentResponse = _squareRepo.ChargePayment(model.SquareupInfo, Amount, customerDetails);
                    if (paymentResponse is SquareupErrorResponse)
                        throw new AppException(((SquareupErrorResponse)paymentResponse).errors.FirstOrDefault().detail);
                }
                if (model.PaymentMethod.ToLower().Equals(EPaymentMode.Paypal.ToString().ToLower()))
                {
                    paymentResponse = _paypalRepo.ChargePayment(model.PaypalInfo, Amount, customerDetails);
                    if (paymentResponse is PaypalErrorResponse)
                        throw new AppException(((PaypalErrorResponse)paymentResponse).Message);
                }

                if (paymentResponse == null)
                    throw new AppException(_localizer["PaymentUnsuccessful"]);

                _customerRepo.MakeAdditionPaymentFromQRScan(model, paymentResponse);

                response.Data = null;
                response.Status = true;
                response.Message = _localizer["PaymentSuccessful"];

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

                _mailService.WDLogError("MakeAdditionalPaymentFromQRScan", ex.Message);

            }
            return response;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("GetParkingLocationGates")]
        public Response GetParkingLocationGates(long ParkingLocationId)
        {
            Response response = new Response();
            try
            {
                if (ParkingLocationId == 0)
                    throw new AppException(_localizer["ParkingLocationIdRequired"]);
				response.Data = _parkingRepo.GetParkingLocationGates(ParkingLocationId);
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
                _mailService.WDLogError("GetParkingLocationGates - ", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("OpenParkingLocationGate")]
        public Response OpenParkingLocationGate(CommonId model)
        {
            Response response = new Response();
            try
            {
                _parkingRepo.OpenCloseGate(model.Id);
                response.Status = true;
                response.Message = _localizer["GateOpenSuccess"];
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
                _mailService.WDLogError("OpenParkingLocationGate", ex.Message);

            }
            return response;
        }

        [AllowAnonymous]
        [HttpGet("GetMonthlyQRBookingInfo")]
        public Response GetMonthlyQRBookingInfo(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, bool IsFullTimeBooking, long? CustomerVehicleId)
        {
            Response response = new Response();
            try
            {
                DateTime datetime;
                bool checkStartTime = DateTime.TryParse(StartTime, out datetime);
                bool checkEndTime = DateTime.TryParse(EndTime, out datetime);
                if (!checkStartTime)
                    throw new AppException(_localizer["InvalidStartTime"]);
                if (!checkEndTime)
                    throw new AppException(_localizer["InvalidEndTime"]);
             
                if (StartTime == EndTime)
                        throw new AppException(_localizer["StartTimeAndEndTimeError"]);

                EndDate = DateTime.Parse(EndDate.ToShortDateString());
                StartDate = DateTime.Parse(StartDate.ToShortDateString());
                TimeSpan ts = (EndDate + TimeSpan.Parse(EndTime)) - (StartDate + TimeSpan.Parse(StartTime));

                if (ts.TotalHours <= 0) throw new AppException(_localizer["DateRangeError"]);
                else
                {
                    ParkingLocDetailsResponse locResponse = _parkingRepo.GetMonthlyQRBookingInfo(ParkingLocationId, CustomerId, StartDate, EndDate, StartTime, EndTime, IsFullTimeBooking, CustomerVehicleId);
                    response.Data = locResponse;

                    if (response.Data == null)
                        throw new AppException(_localizer["LocationDetailsNotFound"]);

                    else
                    {
                        locResponse.BookingMessage = String.Format( _localizer["MonthlyBookingMessage"], locResponse.StartDate.ToString("MMM dd"), locResponse.EndDate.ToString("MMM dd"), (locResponse.StartDate + TimeSpan.Parse(locResponse.StartTime)).ToString("hh:mm tt"), (locResponse.StartDate + TimeSpan.Parse(locResponse.EndTime)).ToString("hh:mm tt"), locResponse.MaxStay);

                    }
                    response.Status = true;
                    response.Message = _localizer["RequestSuccessful"];
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
                _mailService.WDLogError("GetEstimatedBookingDetailsByLoc", ex.Message);
            }
            return response;
        }
    }
}



