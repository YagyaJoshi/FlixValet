using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Models;
using ValetParkingAPI.Resources;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingAPI.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [ApiController]
    [Route("[controller]")]
    public class SuperAdminServicesController : ControllerBase
    {
        private readonly IStaff _staffRepo;
        private readonly IEmail _mailService;
        private readonly ResourceMsgs _resourceMsgs;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public SuperAdminServicesController(IStaff staffRepo, IEmail mailService, IConfiguration configuration, IStringLocalizer<Resource> localizer)
        {
            _staffRepo = staffRepo;
            _mailService = mailService;
            _configuration = configuration;
            _localizer = localizer;
            _resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();

        }

        [HttpGet("GetAllParkingBusinessOwners")]
        public Response GetAllParkingBusinessOwners(string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue)
        {
            Response response = new Response();
            try
            {
                response.Data = _staffRepo.GetAllParkingBusinessOwners(sortColumn, sortOrder, pageNo, pageSize, SearchValue);
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
                _mailService.WDLogError("GetAllParkingBusinessOwners", ex.Message);
            }
            return response;
        }


        [HttpPost("AddParkingBusinessOwner")]
        public Response AddParkingBusinessOwner(ParkingOwnerRequest model)
        {
            Response response = new Response();
            try
            {
                response.Status = true;
                response.Data = new CommonId { Id = _staffRepo.AddParkingBusinessOwner(model, _localizer["AdminBaseUrl"]) };
                response.Message = model.ParkingBusinessOwnerId > 0 ? _localizer["UpdateParkingBusinessOwner"] : _localizer["AddParkingBusinessOwner"];
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
                _mailService.WDLogError("AddParkingBusinessOwner", ex.Message);
            }
            return response;
        }

        [HttpGet("GetParkingOwnerById")]
        public Response GetParkingOwnerById(long ParkingBusinessOwnerId)
        {
            Response response = new Response();
            try
            {
                if (ParkingBusinessOwnerId.Equals(0))
                    throw new AppException(_localizer["ParkingBusinessOwnerIdRequired"]);
                response.Data = _staffRepo.GetParkingOwnerById(ParkingBusinessOwnerId);
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
                _mailService.WDLogError("GetParkingOwnerById", ex.Message);
            }
            return response;
        }

        [HttpPost("DeleteParkingBusinessOwner")]
        public Response DeleteParkingBusinessOwner(ParkingOwnerIdModel model)
        {
            Response response = new Response();
            try
            {
                var (Id, IsActive) = _staffRepo.DeleteParkingBusinessOwner(model);
                response.Data = new CommonId { Id = Id };
                response.Status = true;
                response.Message = string.Format(_localizer["OwnerActiveInactiveMessage"], IsActive ? "activated" : "deactivated");
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
                _mailService.WDLogError("DeleteParkingBusinessOwner", ex.Message);

            }
            return response;
        }


    }
}