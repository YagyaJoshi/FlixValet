using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models.CustomerModels;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingDAL.Models.JobModels;
using ValetParkingBLL.Repository;
using Microsoft.AspNetCore.Authorization;
using ValetParkingDAL.Models;

namespace ValetParkingAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJob _jobRepo;

        public IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;
        private readonly ISquare _squareRepo;
        private readonly IEmail _mailService;

        public JobController(IConfiguration configuration, IJob jobRepo, IEmail mailService, IStringLocalizer<Resource> localizer, ISquare squareRepo)
        {
            _configuration = configuration;
            _localizer = localizer;
            _squareRepo = squareRepo;
            _jobRepo = jobRepo;
            _mailService = mailService;

        }

        [HttpGet("GetRefundPendingList")]
        public Response GetRefundPendingList()
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetRefundPendingStatusList();
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
                _mailService.WDLogError("GetRefundPendingList - ", ex.Message);
            }
            return response;
        }

        [HttpPost("UpdatePendingStatus")]
        public Response UpdatePendingStatus(PendingStatusRequest model)
        {
            Response response = new Response();

            try
            {
                _jobRepo.UpdatePendingStatus(model);
                response.Message = _localizer["AddSuccessful"];
                response.Status = true;

            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("UpdatePendingStatus - ", ex.Message);

            }
            return response;
        }

        [HttpPost("DeleteArchiveAnpr")]
        public Response DeleteArchiveAnpr(ArchiveAnprModel model)
        {
            Response response = new Response();
            try
            {
                _jobRepo.DeleteArchiveAnpr(model);
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
                _mailService.WDLogError("DeleteArchiveAnpr", ex.Message);

            }
            return response;
        }

        [HttpGet("GetRemindingList")]
        public Response GetRemindingList(DateTime CurrentDate)
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetRemindingList(CurrentDate);
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
                // _mailService.WDLogError("GetRemindingLis", ex.Message);
            }
            return response;
        }



        [HttpPost("SaveReminderNotification")]
        public Response SaveReminderNotification(NotificationListModel notificationList)
        {
            Response response = new Response();

            try
            {
                _jobRepo.SaveReminderNotification(notificationList);
                response.Message = _localizer["AddSuccessful"];
                response.Status = true;

            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("SaveReminderNotification", ex.Message);

            }
            return response;
        }

        [HttpGet("GetMonthlyBookingsExpiringToday")]
        public Response GetMonthlyBookingsExpiringToday(DateTime CurrentDate)
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetMonthlyBookingsExpiringToday(CurrentDate);
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
                _mailService.WDLogError("GetMonthlyBookingsExpiringToday", ex.Message);
            }
            return response;
        }


        [HttpGet("GetStaffByLocationId")]
        public Response GetStaffByLocationId(long locationId)
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetStaffByLocationId(locationId);
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
                _mailService.WDLogError("GetMonthlyBookingsExpiringToday", ex.Message);
            }
            return response;
        }

        [HttpGet("GetAllParkingOwners")]
        public Response GetAllParkingOwners()
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetAllParkingOwners();
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
                _mailService.WDLogError("GetAllParkingOwners", ex.Message);
            }
            return response;
        }

        [HttpGet("GetAllBusinessOffice")]
        public Response GetAllBusinessOffice(long parkingBusinessOwnerId)
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.GetAllBusinessOffice(parkingBusinessOwnerId);
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
                _mailService.WDLogError("GetAllParkingOwners", ex.Message);
            }
            return response;
        }

        [HttpGet("ChargeBackCustomerBookingDetails")]
        public Response ChargeBackCustomerBookingDetails(long businessOfficeId, int month, int year)
        {
            Response response = new Response();
            try
            {
                response.Data = _jobRepo.ChargeBackCustomerBookingDetails(businessOfficeId,
             month, year);
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("ChargeBackCustomerBookingDetails - ", ex.Message);
            }
            return response;
        }

        [HttpPost("SaveChargeBackReport")]
        public Response SaveChargeBackReport(ChargeBackReportInput input)
        {
            Response response = new Response();

            try
            {
                _jobRepo.SaveChargeBackReport(input);
                response.Message = _localizer["AddSuccessful"];
                response.Status = true;

            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("SaveChargeBackReport", ex.Message);
            }
            return response;
        }
    }
}
