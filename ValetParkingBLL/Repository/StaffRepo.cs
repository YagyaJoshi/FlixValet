using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.UserModels;
using System.Linq;
using ValetParkingAPI.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Repository
{
    public class StaffRepo : IStaff
    {

        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly ResourceMsgs _resourceMsgs;
        private readonly IEmail _emailService;
        private readonly ParkingHelper _parkingHelper;
        private readonly ICache _cacheRepo;

        public StaffRepo(
            IConfiguration configuration, IEmail emailService, ParkingHelper parkingHelper, ICache cacheRepo)
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();
            _emailService = emailService;
            _parkingHelper = parkingHelper;
            _cacheRepo = cacheRepo;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateUserRequest, User>();
                cfg.CreateMap<ParkingOwnerRequest, User>();
                cfg.CreateMap<User, ParkingOwnerRequest>();
            });
            _mapper = config.CreateMapper();
        }


        public long AddStaff(CreateUserRequest model, string origin)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddStaff");
            try
            {
                string roles = "", ParkingLocations = "";
                // objCmd = new SqlCommand("sp_CheckUserByEmail");
                // objCmd.Parameters.AddWithValue("@Email", model.Email);
                // DataTable dtUser = objSQL.FetchDT(objCmd);

                // if (dtUser != null && Convert.ToInt32(dtUser.Rows[0]["UserExist"]) > 0)
                //     throw new Exception(_localizer["UserExist"]);

                for (int i = 0; i < model.Role.Length; i++)
                    roles += model.Role[i] + (i == model.Role.Length - 1 ? "" : ",");

                for (int i = 0; i < model.ParkingLocations.Length; i++)
                    ParkingLocations += model.ParkingLocations[i] + (i == model.ParkingLocations.Length - 1 ? "" : ",");

                User account = _mapper.Map<User>(model);
                account.OTPToken = randomTokenString();

                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.LastName);
                objCmd.Parameters.AddWithValue("@ProfilePic", model.ProfilePic);
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@Mobile", _parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@OTPToken", account.OTPToken);
                objCmd.Parameters.AddWithValue("@IsActive", account.IsActive);
                objCmd.Parameters.AddWithValue("@Gender", model.Gender);
                objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                objCmd.Parameters.AddWithValue("@Roles", roles);
                objCmd.Parameters.AddWithValue("@ParkingLocations", ParkingLocations);
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@LicenseUrl", model.LicenseUrl);
                objCmd.Parameters.AddWithValue("@LicenseExpiry", model.LicenseExpiry);
                DataTable dtUser = objSQL.FetchDT(objCmd);
                var Error = Convert.ToString(dtUser.Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                string password = Convert.ToString(dtUser.Rows[0]["PasswordHash"]);

                if (string.IsNullOrEmpty(password))
                {
                    Task.Run(() =>
                    {
                        SendPasswordSetEmail(account, origin);
                    });
                }
                // send email

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

        private void SendPasswordSetEmail(User account, string origin)
        {

            string redirectURL = origin + "/session/set-password";
            // _appSettings.ApiDomain;
            redirectURL = redirectURL + "?Token=" + account.OTPToken;
            string MailText = getEmailTemplateText("\\wwwroot\\EmailTemplates\\PasswordSet.html");
            MailText = string.Format(MailText, account.FirstName.Trim(), account.LastName.Trim(), _appSettings.AppName, redirectURL, account.Email);
            _emailService.Send(
                to: account.Email,
                subject: $"Create password request for {_appSettings.AppName}",
                html: $@"{MailText}"
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


        private string randomTokenString()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public StaffMembersResponse GetAllStaffMembers(long ParkingBusinessOwnerId, long UserId, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllStaffMembers");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
                DataTable dtUsrs = objSQL.FetchDT(objCmd);
                if (dtUsrs.Rows.Count > 0)
                {
                    var lstUsers = (from DataRow dr in dtUsrs.Rows
                                    select new StaffMember
                                    {
                                        UserId = Convert.ToInt64(dr["UserId"]),
                                        FirstName = Convert.ToString(dr["FirstName"]),
                                        LastName = Convert.ToString(dr["LastName"]),
                                        Mobile = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                        MobileCode = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                        Email = Convert.ToString(dr["Email"]),
                                        IsActive = Convert.ToBoolean(dr["IsActive"])
                                    }).ToList();

                    return new StaffMembersResponse { StaffMembers = lstUsers, Total = Convert.ToInt32(dtUsrs.Rows[0]["TotalCount"]) };
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

        public long DeleteStaff(CommonId model)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteStaff");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.Id);
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

        public ParkingBusinessOwnerResponse GetAllParkingBusinessOwners(string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllBusinessOwners");
            try
            {
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
                DataTable dtUsrs = objSQL.FetchDT(objCmd);
                if (dtUsrs.Rows.Count > 0)
                {
                    var lstUsers = (from DataRow dr in dtUsrs.Rows
                                    select new PBusinessOwner
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        Name = Convert.ToString(dr["Name"]),
                                        BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
                                        City = Convert.ToString(dr["City"]),
                                        Address = Convert.ToString(dr["Address"]),
                                        Mobile = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                        MobileCode = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                        Email = Convert.ToString(dr["Email"]),
                                        LogoUrl = Convert.ToString(dr["LogoUrl"]),
                                        IsActive = Convert.ToBoolean(dr["IsActive"])
                                    }).ToList();

                    return new ParkingBusinessOwnerResponse { ParkingBusinessOwner = lstUsers, Total = Convert.ToInt32(dtUsrs.Rows[0]["TotalCount"]) };
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

        public long AddParkingBusinessOwner(ParkingOwnerRequest model, string origin)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddParkingBusinessOwner");
            try
            {
                string roles = string.Empty;

                // for (int i = 0; i < model.Roles.Length; i++)
                //     roles += model.Roles[i] + (i == model.Roles.Length - 1 ? "" : ",");
                string OtpToken = randomTokenString();
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.LastName);
                objCmd.Parameters.AddWithValue("@Email", model.Email);
                objCmd.Parameters.AddWithValue("@Mobile", _parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@OTPToken", OtpToken);
                objCmd.Parameters.AddWithValue("@Gender", model.Gender);
                objCmd.Parameters.AddWithValue("@BusinessTitle", model.BusinessTitle);
                objCmd.Parameters.AddWithValue("@Address", model.Address);
                objCmd.Parameters.AddWithValue("@City", model.City);
                objCmd.Parameters.AddWithValue("@StateCode", model.StateCode);
                objCmd.Parameters.AddWithValue("@CountryCode", model.CountryCode);
                objCmd.Parameters.AddWithValue("@ZipCode", model.ZipCode);
                objCmd.Parameters.AddWithValue("@LogoUrl", model.LogoUrl);
                objCmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                // objCmd.Parameters.AddWithValue("@Roles", roles);
                objCmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                DataSet ds = objSQL.FetchDB(objCmd);

                User account = _mapper.Map<User>(model);
                account.OTPToken = OtpToken;

                var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                string password = Convert.ToString(ds.Tables[1].Rows[0]["PasswordHash"]);

                if (string.IsNullOrEmpty(password))
                {
                    Task.Run(() =>
                    {
                        SendPasswordSetEmail(account, origin);
                    });
                }

                return Convert.ToInt64(ds.Tables[1].Rows[0]["ParkingOwnerId"]);
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

        public ParkingOwnerRequest GetParkingOwnerById(long ParkingBusinessOwnerId)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetParkingOwnerById");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);

                DataSet ds = objSQL.FetchDB(objCmd);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var VehicleMasterData = _cacheRepo.CachedVehicleMasterData();

                    StatesMst state = null; Countries country = null;

                    var user = (from DataRow dr in ds.Tables[1].Rows
                                select new User
                                {
                                    Email = Convert.ToString(dr["Email"]),
                                    FirstName = Convert.ToString(dr["FirstName"]),
                                    LastName = Convert.ToString(dr["LastName"]),
                                    DeviceToken = Convert.ToString(dr["DeviceToken"]),
                                    DeviceType = Convert.ToString(dr["DeviceType"]),
                                    Gender = (!string.IsNullOrEmpty(dr["Gender"].ToString())) ? char.Parse(dr["Gender"].ToString()) : 'U',
                                    Mobile = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                    MobileCode = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                                    CreatedBy = Convert.ToInt64(dr["CreatedBy"])

                                }).FirstOrDefault();
                    if (user != null)
                    {

                        var owner = (from DataRow dr in ds.Tables[0].Rows
                                     select new ParkingOwnerRequest
                                     {
                                         ParkingBusinessOwnerId = Convert.ToInt64(dr["Id"]),
                                         BusinessTitle = Convert.ToString(dr["BusinessTitle"]),
                                         Address = Convert.ToString(dr["Address"]),
                                         City = Convert.ToString(dr["City"]),
                                         ZipCode = Convert.ToString(dr["ZipCode"]),
                                         StateCode = dr["StateId"] == DBNull.Value ? null : _parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).StateCode,
                                         State = dr["StateId"] == DBNull.Value ? null : _parkingHelper.GetState(VehicleMasterData.ListStates, Convert.ToInt64(dr["StateId"]), ref state).Name,
                                         Country = dr["CountryId"] == DBNull.Value ? null : _parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).Name,
                                         CountryCode = dr["CountryId"] == DBNull.Value ? null : _parkingHelper.GetCountry(VehicleMasterData.ListCountries, Convert.ToInt64(dr["CountryId"]), ref country).CountryCode,
                                         LogoUrl = Convert.ToString(dr["LogoUrl"]),
                                         CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                         UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"])


                                     }).FirstOrDefault();

                        bool IsGenderAssigned = user.Gender == 'M' | user.Gender == 'F' | user.Gender == 'O';
                        user.Gender = IsGenderAssigned ? user.Gender : 'U';

                        user.BusinessTitle = owner.BusinessTitle;
                        user.ParkingBusinessOwnerId = owner.ParkingBusinessOwnerId;
                        user.LogoUrl = owner.LogoUrl;
                        user.Address = owner.Address;
                        user.State = owner.State;
                        user.StateCode = owner.StateCode;
                        user.Country = owner.Country;
                        user.CountryCode = owner.CountryCode;
                        user.City = owner.City;
                        user.ZipCode = owner.ZipCode;

                        _mapper.Map<User, ParkingOwnerRequest>(user, owner);



                        return owner;
                    }
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


        public CreateUserRequest GetStaffById(long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStaffById");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);

                DataSet ds = objSQL.FetchDB(objCmd);
                var user = (from DataRow dr in ds.Tables[0].Rows
                            select new CreateUserRequest
                            {
                                Email = Convert.ToString(dr["Email"]),
                                FirstName = Convert.ToString(dr["FirstName"]),
                                LastName = Convert.ToString(dr["LastName"]),
                                DeviceToken = Convert.ToString(dr["DeviceToken"]),
                                DeviceType = Convert.ToString(dr["DeviceType"]),
                                Gender = (!string.IsNullOrEmpty(dr["Gender"].ToString())) ? char.Parse(dr["Gender"].ToString()) : 'U',
                                Mobile = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                MobileCode = _parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                IsActive = Convert.ToBoolean(dr["IsActive"]),
                                ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                CreatedBy = Convert.ToInt64(dr["CreatedBy"]),
                                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"]),
                                ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
                                UserId = Convert.ToInt64(dr["Id"]),
                                LicenseUrl = Convert.ToString(dr["LicenseUrl"]),
                                LicenseExpiry = dr["LicenseExpiry"] != DBNull.Value ? Convert.ToDateTime(dr["LicenseExpiry"]) : (DateTime?)null

                            }).FirstOrDefault();

                if (user != null)
                {
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        var roles = (from DataRow dr in ds.Tables[1].Rows
                                     select
                                         Convert.ToInt32(dr["RoleId"])).ToList();
                        user.Role = roles.ToArray();
                    }
                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        var locations = (from DataRow dr in ds.Tables[2].Rows
                                         select
                                             Convert.ToInt64(dr["ParkingLocationId"])).ToList();
                        user.ParkingLocations = locations.ToArray();

                    }
                    bool IsGenderAssigned = user.Gender == 'M' | user.Gender == 'F' | user.Gender == 'O';
                    user.Gender = IsGenderAssigned ? user.Gender : 'U';

                }
                return user;
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

        public StaffCheckinOut CheckIn(StaffCheckinOut model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CheckIn");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@CheckInTime", model.CheckInTime);
                DataSet ds = objSQL.FetchDB(objCmd);


                var Error = Convert.ToString(ds.Tables[0].Rows[0]["Error"]);
                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                var user = (from DataRow dr in ds.Tables[1].Rows
                            select new StaffCheckinOut
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                UserId = Convert.ToInt64(dr["UserId"]),
                                CheckInTime = Convert.ToDateTime(dr["CheckInTime"]),
                                CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CheckOutTime"]),
                                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                            }).FirstOrDefault();

                return user;
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
        public long CheckOut(StaffCheckOutRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CheckOut");
            try
            {
                objCmd.Parameters.AddWithValue("@CheckInId", model.Id);
                objCmd.Parameters.AddWithValue("@CheckOutTime", model.CheckOutTime);

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

        public StaffCheckinOut GetCheckInDetails(long Id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetCheckinDetails");
            try
            {
                objCmd.Parameters.AddWithValue("@Id", Id);
                DataSet ds = objSQL.FetchDB(objCmd);


                var checkInDetails = (from DataRow dr in ds.Tables[0].Rows
                                      select new StaffCheckinOut
                                      {
                                          Id = Convert.ToInt64(dr["Id"]),
                                          UserId = Convert.ToInt64(dr["UserId"]),
                                          CheckInTime = Convert.ToDateTime(dr["CheckInTime"]),
                                          CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CheckOutTime"]),
                                          CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),

                                      }).FirstOrDefault();


                return checkInDetails;
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

        public InspectCheckInResponse InspectUserCheckIn(long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InspectUserCheckIn");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                DataTable dtCheckIn = objSQL.FetchDT(objCmd);

                var user = (from DataRow dr in dtCheckIn.Rows
                            select new InspectCheckInResponse
                            {
                                Id = dr["CheckInId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CheckInId"]),
                                IsCheckedIn = Convert.ToBoolean(dr["IsCheckedIn"]),
                                Message = Convert.ToString(dr["Message"]),
                                CheckInTime = dr["CheckInTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CheckInTime"])
                            }).FirstOrDefault();

                return user;
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


        public (List<string>, long) GetDeviceTokensofLocationStaff(long ParkingLocationId, DateTime CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetDeviceTokensofLocationStaff");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                DataSet dsPDetails = objSQL.FetchDB(objCmd);

                var deviceTokens = (from DataRow dr in dsPDetails.Tables[0].Rows
                                    select Convert.ToString(dr["DeviceToken"])).ToList();

                var BadgeCount = (from DataRow dr in dsPDetails.Tables[1].Rows
                                  select Convert.ToInt64(dr["BadgeCount"])).FirstOrDefault();

                return (deviceTokens, BadgeCount);
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

        public ManagerNotificationModel GetNotificationListForLocation(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, DateTime SearchDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetNotificationListForLocation");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchDate", SearchDate);

                DataTable dtNotification = objSQL.FetchDT(objCmd);
                if (dtNotification.Rows.Count > 0)
                {
                    var lstNotification = (from DataRow dr in dtNotification.Rows
                                           select new ManagerNotification
                                           {
                                               NotificationId = Convert.ToInt64(dr["NotificationId"]),
                                               CustomerName = Convert.ToString(dr["CustomerName"]),
                                               Message = Convert.ToString(dr["Message"]),
                                               ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                               IsAccepted = Convert.ToBoolean(dr["IsAccepted"]),
                                               AcceptedUserId = dr["AcceptedUserId"] != DBNull.Value ?
                                                Convert.ToInt64(dr["AcceptedUserId"]) : (long?)null,
                                               IsBookingCompleted = Convert.ToBoolean(dr["IsBookingCompleted"]),
                                               UnreadCount = Convert.ToInt64(dr["UnreadCount"]),
                                               ShowActionButtons = Convert.ToBoolean(dr["ShowActionButtons"])
                                           }).ToList();

                    return new ManagerNotificationModel { Notifications = lstNotification.OrderByDescending(a => a.UnreadCount).ToList(), Total = Convert.ToInt32(dtNotification.Rows[0]["TotalCount"]) };
                }
                return new ManagerNotificationModel { Notifications = new List<ManagerNotification>(), Total = 0 };
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


        public CheckInCheckOut GetCheckInOutListByUser(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string StartDate, string EndDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetCheckInOutListByUser");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);
                objCmd.Parameters.AddWithValue("@StartDate", StartDate);
                objCmd.Parameters.AddWithValue("@EndDate", EndDate);

                DataTable dtUser = objSQL.FetchDT(objCmd);
                if (dtUser.Rows.Count > 0)
                {
                    var checkin = (from DataRow dr in dtUser.Rows
                                   select new CheckInOut
                                   {
                                       CheckInTime = Convert.ToDateTime(dr["CheckInTime"]),
                                       CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? "-" : Convert.ToString(dr["CheckOutTime"])
                                   }).ToList();

                    checkin.ForEach(a => { if (a.CheckOutTime != "-") a.CheckOutTime = Convert.ToDateTime(a.CheckOutTime); });

                    return new CheckInCheckOut { CheckInOutDetails = checkin, Total = Convert.ToInt32(dtUser.Rows[0]["TotalCount"]) };
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

        public DamageVehicleResponse GetDamageVehicleByParkingOwner(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetDamageVehicleByParkingOwner");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SearchValue", SearchValue);


                DataTable dtUser = objSQL.FetchDT(objCmd);
                if (dtUser.Rows.Count > 0)
                {
                    var damageVehicle = (from DataRow dr in dtUser.Rows
                                         select new DamageVehicle
                                         {
                                             DamageVehicleId = Convert.ToInt64(dr["DamageVehicleId"]),
                                             ValetName = Convert.ToString(dr["ValetName"]),
                                             CustomerName = Convert.ToString(dr["CustomerName"]),
                                             NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                             ReportedDate = Convert.ToDateTime(dr["ReportedDate"])
                                         }).ToList();

                    return new DamageVehicleResponse { DamageVehicleList = damageVehicle, Total = Convert.ToInt32(dtUser.Rows[0]["TotalCount"]) };
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

        public List<GetStaffByOwnerResponse> GetStaffByParkingOwner(long ParkingBusinessOwnerId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStaffByParkingOwner");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);
                DataSet ds = objSQL.FetchDB(objCmd);
                var staff = (from DataRow dr in ds.Tables[0].Rows
                             select new GetStaffByOwnerResponse
                             {
                                 UserId = Convert.ToInt64(dr["UserId"]),
                                 UserName = Convert.ToString(dr["UserName"])
                             }).ToList();

                return staff;
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

        public DamageVehicleDetailsResponse GetDamageVehicleDetails(long DamageVehicleId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetDamageVehicleDetails");
            try
            {
                objCmd.Parameters.AddWithValue("@DamageVehicleId", DamageVehicleId);

                DataSet ds = objSQL.FetchDB(objCmd);
                var details = (from DataRow dr in ds.Tables[0].Rows
                               select new DamageVehicleDetailsResponse
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   CustomerId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
                                   CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
                                   CustomerBookingId = Convert.ToInt64(dr["CustomerBookingId"]),
                                   ValetName = Convert.ToString(dr["ValetName"]),
                                   CustomerName = Convert.ToString(dr["CustomerName"]),
                                   Notes = Convert.ToString(dr["Notes"]),
                                   NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                   VehicleModal = Convert.ToString(dr["VehicleModal"]),
                                   ReportedDate = Convert.ToDateTime(dr["ReportedDate"])
                               }).FirstOrDefault();
                var damageImages = (from DataRow dr in ds.Tables[1].Rows
                                    select new DamageVehicleImages
                                    {

                                        ImageURL = Convert.ToString(dr["Image"])
                                    }).ToList();
                if (details != null)
                    details.Images = damageImages;
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

        public UnDepositedPaymentResponse GetUndepositedPaymentList(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string CurrentDate)
        {
            //DateTime currentDate = DateTime.Parse(CurrentDate);
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUndepositedPaymentList");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                //objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataSet ds = objSQL.FetchDB(objCmd);
                UnDepositedPaymentResponse undepositedList = null;
                List<UnDepositedPaymentList> Undepositedlist = new List<UnDepositedPaymentList>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Undepositedlist = (from DataRow dr in ds.Tables[0].Rows
                                       select new UnDepositedPaymentList
                                       {
                                           PaymentId = Convert.ToInt64(dr["PaymentId"]),
                                           NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                           Amount = Convert.ToDecimal(dr["Amount"]),
                                           Notes = dr["Notes"] == DBNull.Value ? null : Convert.ToString(dr["Notes"])
                                       }).ToList();

                }

                undepositedList = new UnDepositedPaymentResponse
                {
                    UndepositedList = Undepositedlist,
                    Total = Undepositedlist.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]) : 0,
                    TotalAmount = Undepositedlist.Count > 0 ? Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalAmount"]) : 0,
                    LastDepositedAmount = ds.Tables[1].Rows[0]["LastDepositedAmount"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[1].Rows[0]["LastDepositedAmount"]) : (decimal?)null,
                    LastDepositedDate = ds.Tables[1].Rows[0]["LastDepositedDate"] != DBNull.Value ? Convert.ToDateTime(ds.Tables[1].Rows[0]["LastDepositedDate"]) : (DateTime?)null,
                    Symbol = Convert.ToString(ds.Tables[1].Rows[0]["Symbol"])
                };

                return undepositedList;
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

        public decimal UpdateDepositedPayment(UpdateDepositedPayment model)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateDepositedPayment");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@DepositedVia", model.DepositedVia);
                objCmd.Parameters.AddWithValue("@DepositedDate", model.DepositedDate);

                DataTable dtTotalAmount = objSQL.FetchDT(objCmd);

                return Convert.ToDecimal(dtTotalAmount.Rows[0]["TotalAmount"]);
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

        public long AddOwnerPaymentSettings(OwnerPaymentSettings model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOwnerPaymentSettings");
            try
            {
                objCmd.Parameters.AddWithValue("@PaymentSettingsId", model.PaymentSettingsId);
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
                objCmd.Parameters.AddWithValue("@ApiKey", model.ApiKey);
                objCmd.Parameters.AddWithValue("@SecretKey", model.SecretKey);
                objCmd.Parameters.AddWithValue("@AccessToken", model.AccessToken);
                objCmd.Parameters.AddWithValue("@ApplicationId", model.ApplicationId);
                objCmd.Parameters.AddWithValue("@LocationId", model.LocationId);
                objCmd.Parameters.AddWithValue("@IsProduction", model.IsProduction);

                DataTable dtSettings = objSQL.FetchDT(objCmd);

                var Error = Convert.ToString(dtSettings.Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                return Convert.ToInt64(dtSettings.Rows[0]["PaymentSettingsId"]);
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

        public OwnerPaymentSettings GetOwnerPaymentSettings(long ParkingBusinessOwnerId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOwnerPaymentSettings");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);

                DataTable dtSettings = objSQL.FetchDT(objCmd);

                var paymentSettings = (from DataRow dr in dtSettings.Rows
                                       select new OwnerPaymentSettings
                                       {
                                           PaymentSettingsId = Convert.ToInt64(dr["Id"]),
                                           ParkingBusinessOwnerId = Convert.ToInt64(dr["ParkingBusinessOwnerId"]),
                                           PaymentMethod = Convert.ToString(dr["PaymentMethod"]),
                                           ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
                                           SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"]),
                                           AccessToken = dr["AccessToken"] == DBNull.Value ? null : Convert.ToString(dr["AccessToken"]),
                                           ApplicationId = dr["ApplicationId"] == DBNull.Value ? null : Convert.ToString(dr["ApplicationId"]),
                                           LocationId = dr["LocationId"] == DBNull.Value ? null : Convert.ToString(dr["LocationId"]),
                                           IsProduction = Convert.ToBoolean(dr["IsProduction"])
                                       }).FirstOrDefault();

                return paymentSettings;
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
        public long ChangeStaffActiveStatus(StaffActiveInActiveRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ChangeStaffActiveStatus");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objSQL.UpdateDB(objCmd, true);
                return model.UserId;
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

        public (long, bool) DeleteParkingBusinessOwner(ParkingOwnerIdModel model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteParkingOwner");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", model.ParkingBusinessOwnerId);
                DataTable dtOwner = objSQL.FetchDT(objCmd);

                var Error = Convert.ToString(dtOwner.Rows[0]["Error"]);
                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                return (model.ParkingBusinessOwnerId, Convert.ToBoolean(dtOwner.Rows[0]["IsActive"]));
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

