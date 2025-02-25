using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Models;
using ValetParkingAPI.Resources;
using ValetParkingBLL.Interfaces;
using ValetParkingBLL.Repository;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RegionController : ControllerBase
    {
        private readonly IRegion _regionRepo;
        private readonly ResourceMsgs _resourceMsgs;
        private readonly IEmail _mailService;

        public IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public RegionController(IRegion regionRepo, IConfiguration configuration, IEmail mailService, IStringLocalizer<Resource> localizer)
        {
            _configuration = configuration;
            _mailService = mailService;
            _regionRepo = regionRepo;
            _localizer = localizer;
            _resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();

        }

        [HttpPost("add-statecodes")]
        public Response AddStateCodeForCountry(StateCodeRequest stateCodeRequest)
        {
            Response response = new Response();

            try
            {
                _regionRepo.AddStateCodesForCountry(stateCodeRequest.lstStates, stateCodeRequest.CountryCode);
                response.Message = _localizer["AddSuccessful"];
                response.Status = true;

            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("AddStateCodesForCountry - ", ex.Message);

            }
            return response;
        }



        [HttpGet("GetAllCountries")]
        public Response GetAllCountries()
        {
            Response response = new Response();
            try
            {
                response.Data = _regionRepo.GetAllCountries();
                response.Status = true;
                response.Message = _localizer["RequestSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("GetAllCountries - ", ex.Message);
            }
            return response;
        }


        [HttpGet("GetStatesForCountry")]
        public Response GetStatesForCountry(string CountryCode)
        {
            Response response = new Response();
            try
            {
                response.Data = _regionRepo.GetStatesForCountry(CountryCode);
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
                _mailService.WDLogError("GetStatesForCountry - ", ex.Message);
            }
            return response;
        }

        [HttpGet("GetTimeZones")]
        public Response GetTimeZones()
        {
            Response response = new Response();
            try
            {
                response.Data = _regionRepo.GetTimeZones();
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
                _mailService.WDLogError("GetTimeZones - ", ex.Message);
            }
            return response;
        }
    }

}