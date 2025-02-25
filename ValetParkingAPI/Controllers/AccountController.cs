using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.UserModels;
using ValetParkingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;

namespace ValetParkingAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccount _accountService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRegion _regionRepo;
        private readonly IEmail _mailService;
        private readonly ResourceMsgs _resourceMsgs;
        private readonly IStringLocalizer _localizer;

        public AccountController(
            IAccount accountService,
            IMapper mapper,
            IConfiguration configuration,
            IRegion regionRepo,
            IEmail mailService,
             IStringLocalizer<Resource> localizer
            )
        {
            _configuration = configuration;
            _mailService = mailService;
            _regionRepo = regionRepo;
            _accountService = accountService;
            _mapper = mapper;

            _resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();
            _localizer = localizer;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public Response Login(LoginRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.Login(model);
                // setTokenCookie(response.Data.RefreshToken);
                response.Status = true;
                response.Message = _localizer["login"];


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
                _mailService.WDLogError("Login - ", ex.Message);
            }
            return response;
        }

        [AllowAnonymous]
        [HttpPost("Customerlogin")]
        public Response CustomerLogin(LoginRequest model)
        {
            Response response = new Response();
            try
            {


                response.Data = _accountService.CustomerLogin(model);
                response.Status = true;
                response.Message = _localizer["login"];

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
             //   _mailService.WDLogError("CustomerLogin - ", ex.Message);
            }
            return response;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public Response Register(RegisterRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = new CommonId { Id = _accountService.Register(model, _localizer["BaseUrl"]) };
                response.Status = true;
                response.Message = _localizer["RegisterSuccessful"];
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
                _mailService.WDLogError("Register - ", ex.Message);
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public Response VerifyEmail(VerifyEmailRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.VerifyEmail(model.Token, _localizer["BaseUrl"]);
                response.Status = true;
                response.Message = _localizer["EmailVerified"];

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
                _mailService.WDLogError("VerifyEmail - ", ex.Message);
            }
            return response;
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public Response ForgotPassword(ForgotPasswordRequest model)
        {
            Response response = new Response();
            try
            {
                model.BaseUrl = string.IsNullOrEmpty(model.BaseUrl) ? "admin" : model.BaseUrl;
                response.Data = _accountService.ForgotPassword(model, model.BaseUrl.Contains("admin") ? _localizer["AdminBaseUrl"] : _localizer["BaseUrl"]);
                response.Status = true;
                response.Message = _localizer["ForgotPassword"];
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
                _mailService.WDLogError("ForgotPassword - ", ex.Message);
            }
            return response;
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public Response ResetPassword(ResetPasswordRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.ResetPassword(model, _localizer["BaseUrl"]);
                response.Status = true;
                response.Message = _localizer["ResetPassword"];
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
                _mailService.WDLogError("ResetPassworrd - ", ex.Message);
            }
            return response;
        }

        [HttpPost("change-password")]
        public Response ChangePassword(ChangePasswordRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.ChangePassword(model);
                response.Status = true;
                response.Message = _localizer["ChangePassword"];
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
                _mailService.WDLogError("ChangePassword- ", ex.Message);
            }
            return response;
        }

        [AllowAnonymous]
        [HttpPost("set-password")]
        public Response SetPassword(ResetPasswordRequest model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.SetPassword(model);
                response.Status = true;
                response.Message = _localizer["SetPassword"];

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
                _mailService.WDLogError("SetPassword - ", ex.Message);
            }
            return response;
        }




        [HttpPost("add-role")]
        public Response AddRole(Role model)
        {
            Response response = new Response();
            try
            {
                response.Data = _accountService.AddRole(model);
                response.Status = true;
                response.Message = _localizer["AddSuccessful"];
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Status = false;
                response.Message = ex.Message;
                _mailService.WDLogError("AddRole ", ex.Message);

            }

            return response;
        }

        [HttpGet("GetRoles")]
        public Response GetRoles()
        {

            Response response = new Response();

            try
            {

                response.Data = _accountService.GetRoles();
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
                _mailService.WDLogError("GetRoles - ", ex.Message);
            }

            return response;
        }

        [HttpGet("GetUserByEmail")]
        public Response GetUserByEmail(string Email)
        {
            Response response = new Response();
            try
            {
                if (string.IsNullOrEmpty(Email))
                    throw new AppException(_localizer["EmailRequired"]);
                response.Data = _accountService.GetUserByEmail(Email);
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
                _mailService.WDLogError("GetUserByEmail - " + Email, ex.Message);
            }

            return response;
        }


        // helper methods

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

    }
}