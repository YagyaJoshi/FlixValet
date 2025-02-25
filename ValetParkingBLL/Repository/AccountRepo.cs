using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL;
using ValetParkingDAL.Models.UserModels;
using ValetParkingDAL.Enums;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingAPI.Models;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Repository
{
    public class AccountRepo : IAccount
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly ResourceMsgs _resourceMsgs;
        private readonly IEmail _emailService;
        private readonly ParkingHelper _parkingHelper;
        private readonly ICache _cacheRepo;

        // private SQLManager objSQL;
        // private SqlCommand objCmd;


        public AccountRepo(
            IConfiguration configuration, IEmail emailService, ParkingHelper parkingHelper, ICache cacheRepo
            )
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RegisterRequest, User>();
                cfg.CreateMap<User, LoginResponse>();
                cfg.CreateMap<User, AccountResponse>();
                cfg.CreateMap<CreateUserRequest, User>();
                cfg.CreateMap<User, CustomerLoginResponse>();
            });
            _mapper = config.CreateMapper();
            _emailService = emailService;
            _parkingHelper = parkingHelper;
            _cacheRepo = cacheRepo;
        }

        public LoginResponse Login(LoginRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_loginUser");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@OSVersion", model.OSVersion);
                objCmd.Parameters.AddWithValue("@AppVersionCode", model.AppVersionCode);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);
                objCmd.Parameters.AddWithValue("@DeviceName", model.DeviceName);

                DataSet ds = objSQL.FetchDB(objCmd);
                var account = (from DataRow dr in ds.Tables[0].Rows
                               select new User
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   FirstName = dr["FirstName"].ToString(),
                                   LastName = dr["LastName"].ToString(),
                                   PasswordHash = dr["PasswordHash"].ToString(),
                                   ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                   Email = dr["Email"].ToString(),
                                   Mobile = dr["Mobile"].ToString(),
                                   Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                   PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                   ParkingBusinessOwnerId = dr["ParkingBusinessOwnerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
                                   BusinessTitle = dr["BusinessTitle"] == DBNull.Value ? null : Convert.ToString(dr["BusinessTitle"]),
                                   LogoUrl = dr["LogoUrl"] == DBNull.Value ? null : Convert.ToString(dr["LogoUrl"]),
                                   IsActive = Convert.ToBoolean(dr["IsActive"]),
                                   BusinessOfficeId = dr["BusinessOfficeId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["BusinessOfficeId"])
                               }).FirstOrDefault();

                if (account == null || string.IsNullOrEmpty(account.PasswordHash) || !BC.Verify(model.Password, account.PasswordHash))
                    throw new AppException("Email or password is incorrect");

                if (!account.IsActive)
                    throw new AppException("Your account is Inactive");

                if (!account.IsVerified)
                    throw new AppException("Your account is not verified, please verify your account");



                var response = _mapper.Map<LoginResponse>(account);

                if (ds.Tables[1].Rows.Count > 0)
                {
                    var roles = (from DataRow dr in ds.Tables[1].Rows
                                 select
                                     Convert.ToString(dr["Name"])
                                 ).ToList();

                    response.IsSuperAdmin = roles.Any(a => a.Equals(ERoles.SuperAdmin.ToString()));

                    response.Roles = roles;

                    if (model.IsLoginFromValetApp)
                    {
                        bool IsValetorManager = roles.Any(a => a.Equals(ERoles.ParkingManager.ToString())
                        || a.Equals(ERoles.Valet.ToString()));

                        if (!IsValetorManager)
                            throw new AppException("Please login from valet or manager credentials");

                    }
                }



                if (ds.Tables[2].Rows.Count > 0)
                {
                    var locations = (from DataRow dr in ds.Tables[2].Rows
                                     select
                                         Convert.ToInt64(dr["Id"])).ToList();
                    response.ParkingLocations = locations;
                }
                var jwtToken = generateJwtToken(account);
                response.JwtToken = jwtToken;
                return response;
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
        public long Register(RegisterRequest model, string origin)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_RegisterUser");
            try
            {
                // map model to new user object
                var account = _mapper.Map<User>(model);
                account.CreatedDate = DateTime.UtcNow;
                account.VerificationToken = randomTokenString();

                // hash password
                account.PasswordHash = BC.HashPassword(model.Password);

                objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.LastName);
                objCmd.Parameters.AddWithValue("@ProfilePic", model.ProfilePic);
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
                objCmd.Parameters.AddWithValue("@Mobile", _parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@VerificationToken", account.VerificationToken);
                objCmd.Parameters.AddWithValue("@Gender", model.Gender);
                //objSQL.UpdateDB(objCmd, true);
                DataTable dtUser = objSQL.FetchDT(objCmd);

                var Error = Convert.ToString(dtUser.Rows[0]["Error"]);
                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                sendVerificationEmail(account, origin);

                return Convert.ToInt64(dtUser.Rows[0]["Id"]);
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

        public bool VerifyEmail(string token, string origin)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_VerifyUser");
            try
            {
                objCmd.Parameters.AddWithValue("@Token", token);
                DataTable dtUser = objSQL.FetchDT(objCmd);

                // if (Convert.ToInt32(dtCount.Rows[0][0]) == 0)
                if (dtUser.Rows.Count == 0)
                    throw new AppException("Verification failed");

                var account = (from DataRow dr in dtUser.Rows
                               select new User
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   FirstName = dr["FirstName"].ToString(),
                                   LastName = dr["LastName"].ToString(),
                                   PasswordHash = Convert.ToString(dr["PasswordHash"]),
                                   VerificationToken = dr["VerificationToken"].ToString(),
                                   Email = dr["Email"].ToString(),
                                   Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                   ResetToken = dr["ResetToken"].ToString(),
                                   ResetTokenExpires = dr["ResetTokenExpires"] != System.DBNull.Value ? Convert.ToDateTime(dr["ResetTokenExpires"]) : (DateTime?)null,
                                   PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                               }).FirstOrDefault();


                sendMailAfterVerification(account, origin);
                return true;
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

        public bool ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserbyEmail");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@IsFromCustomerApp", model.BaseUrl.Contains("admin") ? false : true);

                DataTable dtUser = objSQL.FetchDT(objCmd);
                var account = (from DataRow dr in dtUser.Rows
                               select new User
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   FirstName = dr["FirstName"].ToString(),
                                   LastName = dr["LastName"].ToString(),
                                   PasswordHash = Convert.ToString(dr["PasswordHash"]),
                                   VerificationToken = dr["VerificationToken"].ToString(),
                                   Mobile = dr["Mobile"].ToString(),
                                   Email = dr["Email"].ToString(),
                                   Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                   ResetToken = dr["ResetToken"].ToString(),
                                   ResetTokenExpires = dr["ResetTokenExpires"] != System.DBNull.Value ? Convert.ToDateTime(dr["ResetTokenExpires"]) : (DateTime?)null,
                                   PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                               }).FirstOrDefault();
                // always return ok response to prevent email enumeration
                if (account == null)
                    throw new AppException("Email id is not registered with us");

                // create reset token that expires after 1 day
                account.ResetToken = randomTokenString();
                account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

                objCmd = new SqlCommand("sp_SetResetTokenForForgotPwd");
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@ResetToken", account.ResetToken);
                objCmd.Parameters.AddWithValue("@ResetTokenExpires", account.ResetTokenExpires);
                objCmd.Parameters.AddWithValue("@IsFromCustomerApp", model.BaseUrl.Contains("admin") ? false : true);
                objSQL.UpdateDB(objCmd, true);

                // send email
                sendPasswordResetEmail(account, model.BaseUrl, origin);
                return true;
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
        public bool ResetPassword(ResetPasswordRequest model, string origin)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserAppliedForResetPwd");
            try
            {
                objCmd.Parameters.AddWithValue("@ResetToken", model.Token);
                objCmd.Parameters.AddWithValue("@PasswordHash", BC.HashPassword(model.Password));
                objCmd.Parameters.AddWithValue("@PasswordReset", DateTime.UtcNow);
                DataSet ds = objSQL.FetchDB(objCmd);

                string Error = Convert.ToString(ds.Tables[1].Rows[0]["Error"]);
                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                var account = (from DataRow dr in ds.Tables[0].Rows
                               select new User
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   FirstName = dr["FirstName"].ToString(),
                                   LastName = dr["LastName"].ToString(),
                                   PasswordHash = Convert.ToString(dr["PasswordHash"]),
                                   VerificationToken = dr["VerificationToken"].ToString(),
                                   Mobile = dr["Mobile"].ToString(),
                                   Email = dr["Email"].ToString(),
                                   Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                   ResetToken = dr["ResetToken"].ToString(),
                                   ResetTokenExpires = dr["ResetTokenExpires"] != System.DBNull.Value ? Convert.ToDateTime(dr["ResetTokenExpires"]) : (DateTime?)null,
                                   PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                               }).FirstOrDefault();

                if (account == null)
                    throw new AppException("Invalid token");

                sendPasswordResetSuccessfulEmail(account, origin);
                return true;
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


        public IEnumerable<AccountResponse> GetAll()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllUsers");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);
                var roles = (from DataRow dr in ds.Tables[0].Rows
                             select new Role
                             {
                                 Id = Convert.ToInt32(dr["Id"]),
                                 Name = Convert.ToString(dr["Name"])
                             }).ToList();

                List<AccountResponse> accountResponse = new List<AccountResponse>();
                foreach (DataRow dr in ds.Tables[1].Rows)
                {

                    AccountResponse accountResponse1 = new AccountResponse();

                    accountResponse1.Id = Convert.ToInt32(dr["Id"]);
                    accountResponse1.FirstName = dr["FirstName"].ToString();
                    accountResponse1.LastName = dr["LastName"].ToString();
                    accountResponse1.Email = dr["Email"].ToString();
                    accountResponse1.CreatedDate = Convert.ToDateTime(dr["CreatedDate"]);
                    accountResponse1.UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null;
                    List<Role> ListRoles = new List<Role>();
                    foreach (var item in Convert.ToString(dr["RoleIDs"]).TrimEnd(',').Split(','))
                    {
                        ListRoles.Add(roles.Where(a => a.Id == Convert.ToInt32(item)).Select(a => a).FirstOrDefault()
                        );
                    }
                    accountResponse1.Roles = ListRoles;


                    accountResponse.Add(accountResponse1);
                }
                return accountResponse;
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

        public void Delete(int Id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteUser");
            try
            {
                objCmd.Parameters.AddWithValue("@Id", Id);
                objSQL.UpdateDB(objCmd);
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

        // helper methods
        public List<Role> GetUserRoles(long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserRoles");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                DataTable dtRole = objSQL.FetchDT(objCmd);
                if (dtRole.Rows.Count > 0)
                {

                    var roles = (from DataRow dr in dtRole.Rows
                                 select new Role
                                 {
                                     Name = Convert.ToString(dr["Name"])
                                 }).ToList();

                    return roles;
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

        // helper methods
        private string generateJwtToken(User account)

        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            account.Roles = GetUserRoles(account.Id);
            var claims = new List<Claim>();

            claims.Add(new Claim("id", account.Id.ToString()));
            foreach (var role in account.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddDays(365),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }




        private string randomTokenString()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private void sendVerificationEmail(User account, string origin = "")
        {

            string redirectURL = !string.IsNullOrEmpty(origin) ? origin : _appSettings.ApiDomain;
            redirectURL = redirectURL + "/verify-email?Token=" + account.VerificationToken;
            string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\RegistrationEmailForVerification.html");
            MailText = string.Format(MailText, account.FirstName.Trim(), account.LastName.Trim(), _appSettings.AppName, redirectURL, account.Email);
            _emailService.Send(
                to: account.Email,
                subject: $"Welcome to {_appSettings.AppName}",
                html: $@"{MailText}"
            );
        }

        private void sendMailAfterVerification(User account, string origin = "")
        {
            string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\RegistrationEmailAfterVerification.html");
            MailText = string.Format(MailText, account.FirstName.Trim(), account.LastName.Trim(), _appSettings.AppName, account.Email);
            _emailService.Send(
                to: account.Email,
                subject: $"Registration successful for {_appSettings.AppName}",
                html: $@"{MailText}"
            );
        }


        private void sendPasswordResetEmail(User account, string BaseUrl, string origin = "")
        {

            string redirectURL;
            if (BaseUrl.Contains("customer"))
                redirectURL = $"{origin}/reset-password?Token={account.ResetToken}";
            else
            {
                redirectURL = $"{origin}/session/reset-password?Token={account.ResetToken}";
            }


            string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\ForgotPasswordEmail.html");
            MailText = string.Format(MailText, account.FirstName.Trim(), account.LastName.Trim(), _appSettings.AppName, redirectURL, account.Email);
            _emailService.Send(
                to: account.Email,
                subject: $"Forgot password request for {_appSettings.AppName}",
                html: $@"<h4>Reset Password Email</h4>
                         {MailText}"
            );
        }

        private void sendPasswordResetSuccessfulEmail(User account, string origin = "")
        {

            string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\ResetPasswordEmail.html");
            MailText = string.Format(MailText, account.FirstName.Trim(), account.LastName.Trim(), _appSettings.AppName, account.Email);
            _emailService.Send(
                to: account.Email,
                subject: $"Password reset successful for { _appSettings.AppName}",
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




        public bool SetPassword(ResetPasswordRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SetPwdForCreatedUser");
            try
            {
                objCmd.Parameters.AddWithValue("@OTPToken", model.Token);
                objCmd.Parameters.AddWithValue("@PasswordHash", BC.HashPassword(model.Password));
                DataTable dt = objSQL.FetchDT(objCmd);

                if (Convert.ToInt32(dt.Rows[0][0]) == 0)
                    throw new AppException("Invalid token");
                return true;
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

        public bool AddRole(Role model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddRoles");
            try
            {
                objCmd.Parameters.AddWithValue("@Name", model.Name);
                DataTable dt = objSQL.FetchDT(objCmd);
                return true;
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

        public List<Role> GetRoles()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetRoles");
            try
            {
                DataTable dtRole = objSQL.FetchDT(objCmd);
                if (dtRole.Rows.Count > 0)
                {

                    var roles = (from DataRow dr in dtRole.Rows
                                 select new Role
                                 {
                                     Id = Convert.ToInt32(dr["Id"]),
                                     Name = Convert.ToString(dr["Name"])
                                 }).ToList();

                    //no need to show superadmin in the roles list.
                    roles = roles.Where(a => !a.Name.Equals(ERoles.SuperAdmin.ToString())).ToList();

                    return roles;
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

        public AccountResponse GetUserByEmail(string Email)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserbyEmail");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", Email);

                DataSet ds = objSQL.FetchDB(objCmd);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var account = (from DataRow dr in ds.Tables[0].Rows
                                   select new User
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       FirstName = dr["FirstName"].ToString(),
                                       LastName = dr["LastName"].ToString(),
                                       ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                       PasswordHash = Convert.ToString(dr["PasswordHash"]),
                                       VerificationToken = dr["VerificationToken"].ToString(),
                                       Email = dr["Email"].ToString(),
                                       Mobile = dr["Mobile"].ToString(),
                                       Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                       ResetToken = dr["ResetToken"].ToString(),
                                       ResetTokenExpires = dr["ResetTokenExpires"] != System.DBNull.Value ? Convert.ToDateTime(dr["ResetTokenExpires"]) : (DateTime?)null,
                                       PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                       CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                       UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                                   }).FirstOrDefault();

                    var roles = (from DataRow dr in ds.Tables[1].Rows
                                 select new Role
                                 {
                                     Id = Convert.ToInt32(dr["Id"]),
                                     Name = Convert.ToString(dr["Name"])
                                 }).ToList();
                    var response = _mapper.Map<AccountResponse>(account);
                    response.Roles = roles;
                    return response;
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

        public bool ChangePassword(ChangePasswordRequest model)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ChangePassword");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@PasswordHash", BC.HashPassword(model.Password));

                DataTable dtUser = objSQL.FetchDT(objCmd);
                string Error = Convert.ToString(dtUser.Rows[0]["Error"]);
                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);
                return true;
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

        public CustomerLoginResponse CustomerLogin(LoginRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CustomerLogin");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@OSVersion", model.OSVersion);
                objCmd.Parameters.AddWithValue("@AppVersionCode", model.AppVersionCode);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);
                objCmd.Parameters.AddWithValue("@DeviceName", model.DeviceName);
                objCmd.Parameters.AddWithValue("@BrowserDeviceToken", model.BrowserDeviceToken);

                DataSet ds = objSQL.FetchDB(objCmd);

                var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

                StatesMst state = null; Countries country = null;

                var account = (from DataRow dr in ds.Tables[0].Rows
                               select new User
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   CustomerId = Convert.ToInt64(dr["CustomerId"]),
                                   FirstName = dr["FirstName"].ToString(),
                                   LastName = dr["LastName"].ToString(),
                                   PasswordHash = dr["PasswordHash"].ToString(),
                                   ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                   Email = dr["Email"].ToString(),
                                   Mobile = dr["Mobile"].ToString(),
                                   Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                                   PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != System.DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                   IsActive = Convert.ToBoolean(dr["IsActive"]),
                                   Address = Convert.ToString(dr["Address"]),
                                   City = Convert.ToString(dr["City"]),
                                   ZipCode = Convert.ToString(dr["ZipCode"]),
                                   State = dr["StateId"] != DBNull.Value ? _parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name : null,
                                   StateCode = dr["StateId"] != DBNull.Value ? _parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode : null,
                                   Country = dr["CountryId"] != DBNull.Value ? _parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name : null,
                                   CountryCode = dr["CountryId"] != DBNull.Value ? _parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode : null,
                                   PaypalCustomerId = Convert.ToString(dr["PaypalCustomerId"])
                               }).FirstOrDefault();

                if (account == null || string.IsNullOrEmpty(account.PasswordHash) || !BC.Verify(model.Password, account.PasswordHash))
                    throw new AppException("Email or password is incorrect");

                if (!account.IsActive)
                    throw new AppException("Your account is Inactive");

                if (!account.IsVerified)
                    throw new AppException("Your account is not verified, please verify your account");


                var response = _mapper.Map<CustomerLoginResponse>(account);

                var jwtToken = generateJwtToken(account);
                response.JwtToken = jwtToken;
                return response;
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

        public bool AddMonthlyCustomerBooking(long bookingId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("InsertMonthlyCustomerBookings");
            try
            {
                objCmd.Parameters.AddWithValue("@BookingId", bookingId);

                DataTable dt = objSQL.FetchDT(objCmd);
                return true;
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