using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.UserModels;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingDAL.Models.QRModels;
using ValetParkingDAL.Models.CustomerModels;

namespace ValetParkingAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ParkingOwnerServicesController : ControllerBase
	{
		private readonly IParking _parkingRepo;
		private readonly IAccount _accountRepo;
		private readonly IRegion _regionRepo;
		private readonly IStaff _staffRepo;
		private readonly IStatistics _statisticsRepo;
		private readonly ICustomer _customerRepo;
		private readonly ResourceMsgs _resourceMsgs;
		public IConfiguration _configuration;
		private readonly IEmail _mailService;
		private readonly ICache _cacheRepo;
		private readonly IStringLocalizer _localizer;


		public ParkingOwnerServicesController(IParking parkingRepo, ICustomer customerRepo, IAccount accountRepo, IRegion regionRepo, IStaff staffRepo, IStatistics statisticsRepo,
			IConfiguration configuration, IEmail mailService, ICache cacheRepo, IStringLocalizer<Resource> localizer)
		{

			_configuration = configuration;
			_mailService = mailService;
			_cacheRepo = cacheRepo;
			_parkingRepo = parkingRepo;
			_accountRepo = accountRepo;
			_regionRepo = regionRepo;
			_staffRepo = staffRepo;
			_statisticsRepo = statisticsRepo;
			_customerRepo = customerRepo;
			_localizer = localizer;
			_resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();

		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddParkingLocation")]
		public Response AddParkingLocation(ParkingLocationRequest model)
		{

			Response response = new Response();
			TimeSpan EBEnterFromTime, EBEnterToTime, EBExitByTime;
            TimeSpan NFEnterFromTime, NFEnterToTime, NFExitByTime;

            try
            {
				if (model.EarlyBirdOffer != null && model.EarlyBirdOffer.Count > 0)
				{
					var EB = model.EarlyBirdOffer.FirstOrDefault();

					if (!string.IsNullOrEmpty(EB.EnterFromTime))
						EBEnterFromTime = TimeSpan.Parse(EB.EnterFromTime);
					else throw new AppException(_localizer["EBFromtimeMissing"]);

					if (!string.IsNullOrEmpty(EB.EnterToTime))
						EBEnterToTime = TimeSpan.Parse(EB.EnterToTime);
					else throw new AppException(_localizer["EBTotimeMissing"]);


					if (!string.IsNullOrEmpty(EB.ExitByTime))
						EBExitByTime = TimeSpan.Parse(EB.ExitByTime);
					else throw new AppException(_localizer["EBExittimeMissing"]);


					if (EBExitByTime < EBEnterToTime || EBExitByTime < EBEnterFromTime)
					{
						throw new AppException(_localizer["ExitByTime"]);
					}
					if (EBEnterToTime < EBEnterFromTime)
					{
						throw new AppException(_localizer["EnterFromTime"]);
					}
				}

                if (model.NightFareOffer != null && model.NightFareOffer.Count > 0)
                {
                    var NF = model.NightFareOffer.FirstOrDefault();

                    if (!string.IsNullOrEmpty(NF.EnterFromTime))
                        NFEnterFromTime = TimeSpan.Parse(NF.EnterFromTime);
                    else throw new AppException(_localizer["NFFromtimeMissing"]);

                    if (!string.IsNullOrEmpty(NF.EnterToTime))
                        NFEnterToTime = TimeSpan.Parse(NF.EnterToTime);
                    else throw new AppException(_localizer["NFTotimeMissing"]);


                    if (!string.IsNullOrEmpty(NF.ExitByTime))
                        NFExitByTime = TimeSpan.Parse(NF.ExitByTime);
                    else throw new AppException(_localizer["NFExittimeMissing"]);
                }

                if (model.ParkingTimings == null || model.ParkingTimings.Count == 0)
				{
					throw new AppException(_localizer["LocationTiming"]);
				}
				if (model.ParkingRates == null || model.ParkingRates.Count == 0)
				{
					throw new AppException(_localizer["LocationRates"]);
				}

                // Check if IsMonthlySubscription is true and PricingPlanId is null or invalid
                if (model.IsMonthlySubscription && (model.PricingPlanId == null || model.PricingPlanId <= 0))
                {
                    throw new AppException(_localizer["PricingPlanIdRequired"]);
                }


                var timeZones = _cacheRepo.CachedTimeZones();

				model.TimeZone = timeZones.Where(e => e.TimeZoneId == model.TimeZoneId).Select(a => a.Name).FirstOrDefault();

				response.Message = model.Id == 0 ? _localizer["AddParkingLocation"] : _localizer["UpdateParkingLocation"];
				response.Data = new CommonId { Id = _parkingRepo.AddParkingLocation(model) };

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
				_mailService.WDLogError("AddParkingLocation - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetParkingLocationById")]
		public Response GetParkingLocationById(long ParkingLocationId)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId.Equals(0))
					throw new AppException(_localizer["ParkingLocationIdRequired"]);
				response.Data = _parkingRepo.GetParkingLocationDetails(ParkingLocationId);
				if (response.Data == null)
					throw new AppException(_localizer["LocationNotFound"]);
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
				_mailService.WDLogError("GetParkingLocationById - " + ParkingLocationId, ex.Message);
			}
			return response;
		}


        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetAllParkingLocations")]
		public Response GetAllParkingLocations(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetAllParkingLocations(ParkingBusinessOwnerId, sortColumn, sortOrder, pageNo, pageSize, LocationsList, SearchValue);
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
				_mailService.WDLogError("GetAllParkingLocations - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddStaff")]
		public Response AddStaff(CreateUserRequest model)
		{
			Response response = new Response();
			try
			{
				if (!string.IsNullOrEmpty(model.LicenseUrl))
				{
					if (model.LicenseExpiry < DateTime.Now)
						throw new AppException(_localizer["InValidLicenseExpiry"]);
				}

				response.Data = new CommonId { Id = _staffRepo.AddStaff(model, _localizer["AdminBaseUrl"]) };
				response.Status = true;
				response.Message = model.UserId > 0 ? _localizer["UpdateStaff"] : _localizer["AddStaff"];
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
				_mailService.WDLogError("AddStaff - ", ex.Message);
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

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetParkingLocationsByOwner")]
		public Response GetParkingLocationsByOwner(long ParkingBusinessOwnerId)
		{
			Response response = new Response();
			try
			{
				if (ParkingBusinessOwnerId.Equals(0))
					throw new AppException(_localizer["ParkingBusinessOwnerIdRequired"]);
				response.Data = _parkingRepo.GetParkingLocationsByOwner(ParkingBusinessOwnerId);
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
				_mailService.WDLogError("GetParkingLocationsByOwner- ", ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetAllStaffMembers")]
		public Response GetAllStaffMembers(long ParkingBusinessOwnerId, long UserId, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.GetAllStaffMembers(ParkingBusinessOwnerId, UserId, sortColumn, sortOrder, pageNo, pageSize, SearchValue);
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
				_mailService.WDLogError("GetAllStaffMembers - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("DeleteParkingLocation")]
		public Response DeleteParkingLocation(ParkingLocationIdModel model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.DeleteParkingLocation(model) };
				response.Status = true;
				response.Message = _localizer["DeleteSuccessful"];
			}
			catch (AppException ex)
			{
				if (ex.Message.ToLower().Contains("deactivated"))
				{
					response.Data = new CommonId { Id = model.ParkingLocationId };
					response.Status = true;
					response.Message = ex.Message;
				}
				else
				{
					response.Data = null;
					response.Status = false;
					response.Message = ex.Message;
				}
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("DeleteParkingLocation - ", ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("DeleteStaff")]
		public Response DeleteStaff(CommonId model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _staffRepo.DeleteStaff(model) };
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
				_mailService.WDLogError("DeleteStaff - " + model.Id, ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetStaffById")]
		public Response GetStaffById(long UserId)
		{
			Response response = new Response();
			try
			{
				if (UserId.Equals(0))
					throw new AppException(_localizer["UserIdRequired"]);
				response.Data = _staffRepo.GetStaffById(UserId);
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
				_mailService.WDLogError("GetStaffById - " + UserId, ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetBookingByParkingOwner")]
		public Response GetBookingByParkingOwner(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BookingType)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetBookingByParkingOwner(ParkingBusinessOwnerId, sortColumn, sortOrder, pageNo, pageSize, SearchValue, BookingType);
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
				_mailService.WDLogError("GetBookingsByParkingOwner- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetBookingDetailsByBookingId")]
		public Response GetBookingDetailsByBookingId(long BookingId)
		{
			Response response = new Response();
			try
			{
				if (BookingId.Equals(0))
					throw new AppException(_localizer["BookingIdRequired"]);
				response.Data = _customerRepo.GetBookingDetailsByBookingId(BookingId);
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
				_mailService.WDLogError("GetBookingDetailsByBookingId - " + BookingId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetCheckInOutListByUser")]
		public Response GetCheckInOutListByUser(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string StartDate, string EndDate)
		{
			Response response = new Response();
			try
			{
				DateTime startDate = DateTime.Parse(StartDate), endDate = DateTime.Parse(EndDate);
				if (startDate > endDate)
					throw new AppException("EndDate cannot be less than StartDate");

				response.Data = _staffRepo.GetCheckInOutListByUser(UserId, sortColumn, sortOrder, pageNo, pageSize, SearchValue, StartDate, EndDate);
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
				_mailService.WDLogError("GetCheckInOutListByUser- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetDamageVehicleByParkingOwner")]
		public Response GetDamageVehicleByParkingOwner(long ParkingBuisnessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue)
		{
			Response response = new Response();
			try
			{
				response.Data = _staffRepo.GetDamageVehicleByParkingOwner(ParkingBuisnessOwnerId, sortColumn, sortOrder, pageNo, pageSize, SearchValue);
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
				_mailService.WDLogError("GetDamageVehicleByParkingOwner- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddOwnerPaymentSettings")]
		public Response AddOwnerPaymentSettings(OwnerPaymentSettings model)
		{
			Response response = new Response();
			try
			{

				response.Data = new CommonId { Id = _staffRepo.AddOwnerPaymentSettings(model) };
				response.Status = true;
				response.Message = model.PaymentSettingsId.Equals(0) ? _localizer["PaymentSettingsAdded"] : _localizer["PaymentSettingsUpdated"];
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
				_mailService.WDLogError(" AddOwnerPaymentSettings", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetOwnerPaymentSettings")]
		public Response GetOwnerPaymentSettings(long ParkingBuisnessOwnerId)
		{
			Response response = new Response();
			try
			{
				var paymentSettings = _staffRepo.GetOwnerPaymentSettings(ParkingBuisnessOwnerId);
				// if (paymentSettings == null)
				//     throw new AppException(_localizer["RecordNotFound"]);
				response.Data = paymentSettings;
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
				_mailService.WDLogError("GetOwnerPaymentSettings", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("ChangeStaffActiveStatus")]
		public Response ChangeStaffActiveStatus(StaffActiveInActiveRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _staffRepo.ChangeStaffActiveStatus(model) };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("ChangeStaffActiveStatus- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddLocationCameraSettings")]
		public Response AddLocationCameraSettings(LocationCameraSettings model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.AddLocationCameraSettings(model) };
				response.Status = true;
				response.Message = model.LocationCameraId > 0 ? _localizer["CameraSettingsUpdated"] : _localizer["CameraSettingsAdded"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddLocationCameraSettings - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetLocationCameraSettings")]
		public Response GetLocationCameraSettings(long LocationCameraId)
		{
			Response response = new Response();
			try
			{
				if (LocationCameraId.Equals(0))
					throw new AppException(_localizer["LocationCameraIdRequired"]);
				response.Data = _parkingRepo.GetLocationCameraSettings(LocationCameraId);
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
				_mailService.WDLogError("GetLocationCameraSettings", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetRecognizedVehicleList")]
		public Response GetRecognizedVehicleList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, DateTime CurrentDate, string CameraId)

		{
			Response response = new Response();
			try
			{
				if (string.IsNullOrEmpty(CameraId))
					throw new AppException(_localizer["CameraIdRequired"]);
				response.Data = _customerRepo.GetRecognizedVehicleList(ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, SearchValue, CurrentDate, CameraId);
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
				_mailService.WDLogError("GetRecognizedVehicleListByOwner", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetCameraListByLocation")]
		public Response GetCameraListByLocation(long ParkingLocationId)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetCameraListByLocation(ParkingLocationId);
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
				_mailService.WDLogError("GetCameraIdListByLocation - " + ParkingLocationId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetCameraSettingList")]
		public Response GetCameraSettingList(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue)

		{
			Response response = new Response();
			try
			{

				response.Data = _parkingRepo.GetCameraSettingList(ParkingBusinessOwnerId, ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, SearchValue);
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
				_mailService.WDLogError("GetCameraSettingList", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("DeleteCameraSettings")]
		public Response DeleteCameraSettings(LocationCameraIdModel model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.DeleteCameraSettings(model) };
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
				_mailService.WDLogError("DeleteCameraSettings - " + model.LocationCameraId, ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetLocationQRCode")]
		public Response GetLocationQRCode(long ParkingLocationId, string LogoUrl)
		{
			Response response = new Response();
			try
			{

				response.Data = new { ImageUrl = _parkingRepo.GetLocationQRCode(ParkingLocationId, LogoUrl) };
				if (response.Data == null)
					throw new AppException(_localizer["TigerQRNotGenerated"]);
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
				_mailService.WDLogError("GetLocationQRCode", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("GenerateDynamicQR")]
		public Response GenerateDynamicQR(DynamicQRCodeModel model)
		{
			Response response = new Response();
			try
			{

				response.Data = new { ImageUrl = _parkingRepo.GenerateDynamicQR(model) };
				if (response.Data == null)
					throw new AppException(_localizer["TigerQRNotGenerated"]);
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
				_mailService.WDLogError("GetLocationQRCode", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetLocationQRList")]
		public Response GetLocationQRList(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, bool isMonthly = false)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetLocationQRList(ParkingBusinessOwnerId, sortColumn, sortOrder, pageNo, pageSize, isMonthly);
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
				_mailService.WDLogError("GetLocationQRList - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetOwnerLocationsWithoutQR")]
		public Response GetOwnerLocationsWithoutQR(long ParkingBusinessOwnerId, bool isMonthly = false)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetOwnerLocationsWithoutQR(ParkingBusinessOwnerId, isMonthly);
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
				_mailService.WDLogError("GetOwnerLocationsWithoutQR", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("KeyStatistics")]
		public Response KeyStatistics(long ParkingBusinessOwnerId, string CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (ParkingBusinessOwnerId == 0) throw new AppException(_localizer["ParkingBusinessOwnerIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				response.Data = _statisticsRepo.KeyStatistics(ParkingBusinessOwnerId, CurrentDate);
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
				_mailService.WDLogError("KeyStatistics", ex.Message);
			}
			return response;
		}


        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("KeyStatistics_PieGraphs")]
		public Response KeyStatistics_PieGraphs(long ParkingLocationId, string CurrentDate, string Filter)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				if (string.IsNullOrEmpty(Filter))
					throw new AppException(_localizer["FilterRequired"]);
				response.Data = _statisticsRepo.KeyStatistics_PieGraphs(ParkingLocationId, CurrentDate, Filter);
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
				_mailService.WDLogError("KeyStatistics_PieGraphs", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("RevenueGraph_v1")]
		public Response RevenueGraph_v1(long ParkingLocationId, string CurrentDate, string Filter)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				if (string.IsNullOrEmpty(Filter))
					throw new AppException(_localizer["FilterRequired"]);
				response.Data = _statisticsRepo.RevenueGraph_v1(ParkingLocationId, CurrentDate, Filter);
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
				_mailService.WDLogError("RevenueGraph_v1", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("TransactionsGraph_v1")]
		public Response TransactionsGraph_v1(long ParkingLocationId, string CurrentDate, string Filter)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				if (string.IsNullOrEmpty(Filter))
					throw new AppException(_localizer["FilterRequired"]);
				response.Data = _statisticsRepo.TransactionsGraph_v1(ParkingLocationId, CurrentDate, Filter);
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
				_mailService.WDLogError("TransactionsGraph_v1", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("OccupancyGraph")]
		public Response OccupancyGraph(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				if (string.IsNullOrEmpty(Filter))
					throw new AppException(_localizer["FilterRequired"]);
				response.Data = _statisticsRepo.OccupancyGraph(ParkingLocationId, CurrentDate, Filter, sortColumn, sortOrder, pageNo, pageSize);
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
				// _mailService.WDLogError("DurationsGraph", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("LiveReport")]
		public Response LiveReport(long ParkingLocationId, string CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);

				response.Data = _statisticsRepo.LiveReport(ParkingLocationId, CurrentDate);
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
				_mailService.WDLogError("LiveReport", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("LiveReport_v1")]
		public Response LiveReport_v1(long ParkingLocationId, string CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);

				response.Data = _statisticsRepo.LiveReport_v1(ParkingLocationId, CurrentDate);
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
				_mailService.WDLogError("LiveReport", ex.Message);
			}
			return response;
		}


        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("DurationsGraphv1")]
		public Response DurationsGraphv1(long ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);

				response.Data = _statisticsRepo.DurationsGraphv1(ParkingLocationId, CurrentDate, sortColumn, sortOrder, pageNo, pageSize);
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
				//  _mailService.WDLogError("DurationsGraph", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("OccupancyGraphv1")]
		public Response OccupancyGraphv1(long ParkingLocationId, string CurrentDate, string Filter, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				if (string.IsNullOrEmpty(Filter))
					throw new AppException(_localizer["FilterRequired"]);
				response.Data = _statisticsRepo.OccupancyGraphv1(ParkingLocationId, CurrentDate, Filter, sortColumn, sortOrder, pageNo, pageSize);
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
				// _mailService.WDLogError("DurationsGraph", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("AccountReconcilationv1")]
		public Response AccountReconcilationv1(long ParkingLocationId, string CurrentDate, string depositsortColumn, string depositsortOrder, int? depositpageNo, int? depositpageSize, string bookingsortColumn, string bookingsortOrder, int? bookingpageNo, int? bookingpageSize)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0) throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);

				response.Data = _statisticsRepo.AccountReconcilation_v1(ParkingLocationId, CurrentDate, depositsortColumn, depositsortOrder, depositpageNo, depositpageSize, bookingsortColumn, bookingsortOrder, bookingpageNo, bookingpageSize);
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
				_mailService.WDLogError("AccountReconcilation", ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("FetchCustomerDetails")]

		public Response FetchCustomerDetails(FetchCustomerDetailsRequest model)
		{
			Response response = new Response();
			try
			{
				if (model.BookingType.ToLower().Equals("monthly"))
				{
					if (model.StartTime == model.EndTime)
						throw new AppException(_localizer["StartTimeAndEndTimeError"]);
				}
				model.EndDate = DateTime.Parse(model.EndDate.ToShortDateString());
				model.StartDate = DateTime.Parse(model.StartDate.ToShortDateString());
				TimeSpan ts = (model.EndDate + TimeSpan.Parse(model.EndTime)) - (model.StartDate + TimeSpan.Parse(model.StartTime));
				if (ts.TotalHours <= 0) throw new AppException(_localizer["DateRangeError"]);

				var timeZones = _cacheRepo.CachedTimeZones();
				model.TimeZone = timeZones.Where(e => e.TimeZoneId == model.TimeZoneId).Select(a => a.Name).FirstOrDefault();

				response.Data = _customerRepo.FetchCustomerDetails(model, _localizer["BaseUrl"]);
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
				_mailService.WDLogError("FetchCustomerDetails- ", ex.Message);
			}
			return response;
		}


        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetBookingExtensionPendingList")]
		public Response GetBookingExtensionPendingList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string CurrentDate)
		{
			Response response = new Response();
			try
			{
				if (ParkingLocationId == 0)
					throw new AppException(_localizer["ParkingLocationIdRequired"]);
				if (string.IsNullOrEmpty(CurrentDate))
					throw new AppException(_localizer["DateRequired"]);
				response.Data = _parkingRepo.GetBookingExtensionPendingList(ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, SearchValue, CurrentDate);
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
				_mailService.WDLogError("GetBookingEntensionPendingLis- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("SearchCustomersFromFilter")]
		public Response SearchCustomersFromFilter(string Email, string Mobile)
		{
			Response response = new Response();
			try
			{
				//  if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Mobile))
				//     throw new AppException(_localizer["MobileOrEmail"]);
				response.Data = _customerRepo.SearchCustomersFromFilter(Email, Mobile);
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
				_mailService.WDLogError("SearchCustomersFromFilterResponse- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("BookingRevenueReport")]
		public Response BookingRevenueReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string BookingType, string StartDate, string EndDate)
		{
			Response response = new Response();
			try
			{
				DateTime datetime;
				bool checkStartDate = DateTime.TryParse(StartDate, out datetime);
				bool checkEndDate = DateTime.TryParse(EndDate, out datetime);
				if (!checkStartDate)
					throw new AppException(_localizer["InvalidStartDate"]);
				if (!checkEndDate)
					throw new AppException(_localizer["InvalidStartDate"]);

				response.Data = _parkingRepo.BookingRevenueReport(ParkingBusinessOwnerId, ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, BookingType, StartDate, EndDate);
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
				_mailService.WDLogError("BookingRevenueReport- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetBookingRevenueCSVReport")]
        public Response GetBookingRevenueCSVReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string BookingType, string StartDate, string EndDate)
        {
            Response response = new Response();
            try
            {
                DateTime datetime;
                bool checkStartDate = DateTime.TryParse(StartDate, out datetime);
                bool checkEndDate = DateTime.TryParse(EndDate, out datetime);
                if (!checkStartDate)
                    throw new AppException(_localizer["InvalidStartDate"]);
                if (!checkEndDate)
                    throw new AppException(_localizer["InvalidStartDate"]);

                response.Data = _parkingRepo.GetBookingRevenueCSVReport(ParkingBusinessOwnerId, ParkingLocationId, BookingType, StartDate, EndDate);
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
                _mailService.WDLogError("BookingRevenueReport- ", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("AccountReceivableReport")]
		public Response AccountReceivableReport(long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BusinessOffice, string StartDate, string EndDate, long BookingCategoryId)
		{
			Response response = new Response();
			try
			{
				DateTime datetime;
				bool checkStartDate = DateTime.TryParse(StartDate, out datetime);
				bool checkEndDate = DateTime.TryParse(EndDate, out datetime);
				if (!checkStartDate)
					throw new AppException(_localizer["InvalidStartDate"]);
				if (!checkEndDate)
					throw new AppException(_localizer["InvalidStartDate"]);

				response.Data = _parkingRepo.AccountReceivableReport(ParkingLocationId, sortColumn, sortOrder, pageNo, pageSize, SearchValue, BusinessOffice, StartDate, EndDate, BookingCategoryId);
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
				_mailService.WDLogError("AccountReceivableReport- ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddParkingOwnerBusinessOffice")]
		public Response AddParkingOwnerBusinessOffice(ParkingOwnerBusinessOffices model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.AddParkingOwnerBusinessOffice(model, _localizer["BaseUrl"]) };
				response.Status = true;
				response.Message = model.BusinessOfficeId > 0 ? _localizer["BusinessOfficeUpdated"] : _localizer["BusinessOfficeAdded"];
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
				_mailService.WDLogError("AddParkingOwnerBusinessOffice - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpPost("AddPOBusinessOfficeEmployee")]
		public Response AddPOBusinessOfficeEmployee(POBusinessOfficeEmployees model)
		{
			Response response = new Response();
			try
			{
				bool check = _customerRepo.CheckBuisnessOfficeEmployeeExists(model.BusinessOfficeEmployeeId, model.CustomerVehicleId, model.BusinessOfficeId);
				if (check)
				{
					response.Status = false;
					response.Message = _localizer["BusinessOfficeEmployeeExists"];
				}
				else
				{
					response.Data = new CommonId { Id = _customerRepo.AddPOBusinessOfficeEmployee(model) };
					response.Status = true;
					response.Message = model.BusinessOfficeEmployeeId > 0 ? _localizer["EmployeeUpdated"] : _localizer["EmployeeAdded"];
				}
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddPOBusinessOfficeEmployee - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpPost("AddPOBusinessOfficeEmployeeV1")]
        public Response AddPOBusinessOfficeEmployee_v1(POBusinessOfficeEmployeeInput model)
        {
            Response response = new Response();
            try
            {
                bool check = _customerRepo.CheckBuisnessOfficeEmployeeExists(model.BusinessOfficeEmployeeId, model.CustomerVehicleId, model.BusinessOfficeId);
                if (check)
                {
                    response.Status = false;
                    response.Message = _localizer["BusinessOfficeEmployeeExists"];
                }
                else
                {

                    response.Data = new CommonId { Id = _customerRepo.AddPOBusinessOfficeEmployee_v1(model) };
                    response.Status = true;
                    response.Message = model.BusinessOfficeEmployeeId > 0 ? _localizer["EmployeeUpdated"] : _localizer["EmployeeAdded"];
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("AddPOBusinessOfficeEmployee_v1 - ", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddWhiteListCustomers")]
		public Response AddWhiteListCustomers(WhiteListCustomers model)
		{
			Response response = new Response();
			try
			{
				bool check = _customerRepo.CheckWhiteListVehicleExists(model.WhiteListCustomerId, model.NumberPlate);
				if (check)
				{
					response.Status = false;
					response.Message = _localizer["VehicleAlreadyRegistered"];
				}
				else
				{
					response.Data = new CommonId { Id = _customerRepo.AddWhiteListCustomer(model) };
					response.Status = true;
					response.Message = model.WhiteListCustomerId > 0 ? _localizer["WhiteListCustomerUpdated"] : _localizer["WhiteListCustomerAdded"];
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
				_mailService.WDLogError("AddWhiteListCustomers - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpPost("AddCustomer")]
		public Response AddCustomer(AddCustomerRequest model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _customerRepo.AddCustomer(model, _localizer["BaseUrl"]) };
				response.Status = true;
				response.Message = _localizer["CustomerAdded"];
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
				_mailService.WDLogError("AddCustomer - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetWhiteListCustomerById")]
		public Response GetWhiteListCustomerById(long WhiteListCustomerId)
		{
			Response response = new Response();
			try
			{
				if (WhiteListCustomerId.Equals(0))
					throw new AppException(_localizer["CustomerIdRequired"]);
				response.Data = _customerRepo.GetWhiteListCustomerById(WhiteListCustomerId);
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
				_mailService.WDLogError("GetWhiteListCustomerById - " + WhiteListCustomerId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetWhiteListCustomerList")]
		public Response GetWhiteListCustomerList(int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue, long ParkingBusinessOwnerId)
		{
			Response response = new Response();
			try
			{
				response.Data = _customerRepo.GetWhiteListCustomerList(PageNo, PageSize, SortColumn, SortOrder, SearchValue, ParkingBusinessOwnerId);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetWhiteListCustomerList - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("GetPOBusinessOfficeList")]
        public Response GetPOBusinessOfficeList(long ParkingLocationId)
        {
            Response response = new Response();
            try
            {
                if (ParkingLocationId.Equals(0))
                    throw new AppException(_localizer["ParkingLocationIdRequired"]);
                response.Data = _parkingRepo.GetPOBusinessOfficeList(ParkingLocationId);
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
                _mailService.WDLogError("GetPOBusinessOfficeList - " + ParkingLocationId, ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("GetAllPOBusinessOfficeList")]
        public Response GetAllPOBusinessOfficeList(long? ParkingBusinessOwnerId, long? ParkingLocationId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue)
        {
			Response response = new Response();
			try
			{
                if (ParkingBusinessOwnerId.Equals(0))
                    throw new AppException(_localizer["ParkingBusinessOwnerIdRequired"]);
                response.Data = _parkingRepo.GetAllPOBusinessOfficeList(ParkingBusinessOwnerId, ParkingLocationId, PageNo,  PageSize, SortColumn, SortOrder, SearchValue);
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
				_mailService.WDLogError("GetPOBusinessOfficeList - " + ParkingLocationId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetPOBusinessOfficeById")]
		public Response GetPOBusinessOfficeById(long BusinessOfficeId)
		{
			Response response = new Response();
			try
			{
				if (BusinessOfficeId.Equals(0))
					throw new AppException(_localizer["BusinessOfficeIdRequired"]);
				response.Data = _parkingRepo.GetPOBusinessOfficeById(BusinessOfficeId);
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
				_mailService.WDLogError("GetPOBusinessOfficeById - " + BusinessOfficeId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("GetPOBusinessOfficeEmployeeList")]
		public Response GetPOBusinessOfficeEmployeeList(long ParkingBusinessOwnerId, long? BusinessOfficeId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue)
		{
			Response response = new Response();
			try
			{
				response.Data = _customerRepo.GetPOBusinessOfficeEmployeeList(ParkingBusinessOwnerId, BusinessOfficeId, PageNo, PageSize, SortColumn, SortOrder, SearchValue);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetPOBusinessOfficeEmployeeList - " + ParkingBusinessOwnerId, ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("ChangeBusinessOfficeActiveStatus")]
		public Response ChangeBusinessOfficeActiveStatus(BusinessOfficeIdModel model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.ChangeBusinessOfficeActiveStatus(model) };
				response.Status = true;
				response.Message = _localizer["UpdateSuccessful"];
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
				_mailService.WDLogError("ChangeBusinessOfficeActiveStatus - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("UpdateOfficeEmployeePayment")]
		public Response UpdateOfficeEmployeePayment(OfficeEmployeeListModel model)
		{
			Response response = new Response();
			try
			{
				_customerRepo.UpdateOfficeEmployeePayment(model);
				response.Status = true;
				response.Message = _localizer["UpdateOfficeEmployeePayment"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateOfficeEmployeePayment", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetRelayTypes")]
		public Response GetRelayTypes()
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetRelayTypes();
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetRelayTypes - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("AddParkingLocationGateSettings")]
		public Response AddParkingLocationGateSettings(ParkingLocationGateSettings model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.AddPakingLocationGateSettings(model) };
				response.Status = true;
				response.Message = model.Id > 0 ? _localizer["GateSettingsAdded"] : _localizer["GateSettingsUpdated"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("AddParkingLocationGateSettings - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetAllParkingLocationGateSettings")]
		public Response GetAllParkingLocationGateSettings(long ParkingBusinessOwnerId, int pageNo, int? pageSize, string sortColumn, string sortOrder, string searchValue)
		{
			Response response = new Response();
			try
			{
				if (ParkingBusinessOwnerId.Equals(0))
					throw new AppException("Invalid ParkingBusinessOwnerId");
				response.Data = _parkingRepo.GetAllParkingLocationGateSettings(ParkingBusinessOwnerId, pageNo, pageSize, sortColumn, sortOrder, searchValue);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetAllParkingLocationGateSettings - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetParkingLocationGateSettingById")]
		public Response GetParkingLocationGateSettingById(long Id)
		{
			Response response = new Response();
			try
			{
				response.Data = _parkingRepo.GetParkingLocationGateSettingById(Id);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetParkingLocationGateSettingById - ", ex.Message);
			}
			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpPost("DeleteParkingLocationGateSettings")]
		public Response DeleteParkingLocationGateSettings(CommonId model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _parkingRepo.DeleteParkingLocationGateSetting(model) };
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
				_mailService.WDLogError("DeleteParkingLocationGateSettings - " + model.Id, ex.Message);
			}

			return response;
		}

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor")]
        [HttpGet("GetParkingGateSettingLocationList")]
        public Response GetParkingGateSettingLocationList(long ParkingBusinessOwnerId)
        {
            Response response = new Response();
            try
            {
                response.Data = _parkingRepo.GetParkingGateSettingLocationList(ParkingBusinessOwnerId);
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("GetRelayTypes - ", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("GetBusinessOfficeByUserId")]
        public Response GetBusinessOfficeByUserId(long UserId)
        {
            Response response = new Response();
            try
            {
                response.Data = _parkingRepo.GetBusinessOfficeByUserId(UserId);
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("GetBusinessOfficeByUserId - ", ex.Message);
            }
            return response;
        }

        [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
        [HttpGet("ChargeBackCustomerBookingReport")]
        public Response ChargeBackCustomerBookingReport(long businessOfficeId, int pageNo, int? pageSize, string sortColumn, string sortOrder)
        {
            Response response = new Response();
            try
            {
                response.Data = _parkingRepo.ChargeBackCustomerBookingReport(businessOfficeId,
              pageNo, pageSize, sortColumn, sortOrder);
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("ChargeBackCustomerBookingReport - ", ex.Message);
            }
            return response;
        }

		//    [Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
		//    [HttpGet("GetQrActivityLogs")]
		//    public Response GetQRCodeActivityLogs(string qrId, string timeZone, string date)
		//    {
		//        Response response = new Response();
		//        try
		//        {
		//if (string.IsNullOrEmpty(qrId))
		//	throw new AppException("qr code id is required");
		//            if (string.IsNullOrEmpty(timeZone))
		//                throw new AppException("timeZone is required");
		//            response.Data = _parkingRepo.GetQrCodeActivitylogs(qrId, timeZone, date);
		//            response.Status = true;
		//            response.Message = _localizer["RequestSuccessful"];
		//        }
		//        catch (AppException ex)
		//        {
		//            response.Data = null;
		//            response.Status = false;
		//            response.Message = ex.Message;
		//        }
		//        catch (Exception ex)
		//        {
		//            response.Data = null;
		//            response.Status = false;
		//            response.Message = ex.Message;
		//            _mailService.WDLogError("GetQrActivityLogs - ", ex.Message);
		//        }
		//        return response;
		//    }

		[Authorize(Roles = "SuperAdmin,ParkingAdmin,ParkingSupervisor,BusinessOffice")]
		[HttpGet("GetQrActivityLogs")]
		public Response GetQRCodeActivityLogs_v1(long parkingLocationId, string bookingType, string date)
		{
			Response response = new Response();
			try
			{
				if (parkingLocationId <= 0)
					throw new AppException("ParkingLocationId is required");
				response.Data = _parkingRepo.GetQrCodeActivitylogs(parkingLocationId, bookingType, date);
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
				_mailService.WDLogError("GetQrActivityLogs - ", ex.Message);
			}
			return response;
		}
	}
}