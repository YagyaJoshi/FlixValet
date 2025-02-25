using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models;
using System.Linq;
using System;
using ValetParkingDAL.Models.StateModels;
using ValetParkingDAL.Models.UserModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingAPI.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.NumberPlateRecogModels;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using ValetParkingDAL.Enums;

namespace ValetParkingBLL.Repository
{
    public class MasterRepo : IMaster
    {
        private readonly IConfiguration _configuration;
        private readonly ParkingHelper parkingHelper;

        public MasterRepo(
            IConfiguration configuration, ParkingHelper parkingHelper
            )
        {

            _configuration = configuration;
            this.parkingHelper = parkingHelper;
        }
        public List<ParkingLocationName> GetLocationsByUser(long ParkingBusinessOwnerId, long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetLocationsByUser");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", ParkingBusinessOwnerId);

                DataSet dsLocations = objSQL.FetchDB(objCmd);
                var Error = Convert.ToString(dsLocations.Tables[0].Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

                var lstlocations = (from DataRow dr in dsLocations.Tables[1].Rows
                                    select new ParkingLocationName
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        LocationName = Convert.ToString(dr["LocationName"]),
                                        Currency = Convert.ToString(dr["Currency"]),
                                        CurrencySymbol = Convert.ToString(dr["CurrencySymbol"]),
                                        QrCodePath = Convert.ToString(dr["QrCodePath"]),
                                        TimeZone = Convert.ToString(dr["TimeZone"]),
                                        QRCodePathMonthly = Convert.ToString(dr["QRCodePathMonthly"]),
                                    }).ToList();

                return lstlocations;
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

        public long EditUserProfile(ProfileEdit model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_EditUserProfile");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.LastName);
                objCmd.Parameters.AddWithValue("@ProfilePic", model.ProfilePic);
                objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(model.Mobile));
                objCmd.Parameters.AddWithValue("@Gender", model.Gender);
                objCmd.Parameters.AddWithValue("@IsFromCustomerApp", model.IsFromCustomerApp);
                DataTable dt = objSQL.FetchDT(objCmd);

                var Error = Convert.ToString(dt.Rows[0]["Error"]);

                if (!string.IsNullOrEmpty(Error))
                    throw new AppException(Error);

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

        public ProfileEdit GetUserProfileDetails(long UserId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserProfileDetails");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                DataTable dtUserProfile = objSQL.FetchDT(objCmd);
                var userProfile = (from DataRow dr in dtUserProfile.Rows
                                   select new ProfileEdit
                                   {
                                       Email = Convert.ToString(dr["Email"]),
                                       FirstName = Convert.ToString(dr["FirstName"]),
                                       LastName = Convert.ToString(dr["LastName"]),
                                       Gender = (!string.IsNullOrEmpty(dr["Gender"].ToString())) ? char.Parse(dr["Gender"].ToString()) : 'U',
                                       Mobile = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[1],
                                       MobileCode = parkingHelper.GetMobileSplitedValues(Convert.ToString(dr["Mobile"]))[0],
                                       ProfilePic = Convert.ToString(dr["ProfilePic"]),
                                       UserId = Convert.ToInt64(dr["Id"])

                                   }).FirstOrDefault();
                if (userProfile != null)
                {
                    bool IsGenderAssigned = userProfile.Gender == 'M' | userProfile.Gender == 'F' | userProfile.Gender == 'O';
                    userProfile.Gender = IsGenderAssigned ? userProfile.Gender : 'U';
                }
                return userProfile;
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

        public AppVersionResponse GetAppVersion(AppVersionModel model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAppVersion");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@AppVersionName", model.AppVersionName);
                objCmd.Parameters.AddWithValue("@AppVersionCode", model.AppVersionCode);
                objCmd.Parameters.AddWithValue("@DeviceType", model.DeviceType);
                objCmd.Parameters.AddWithValue("@DeviceName", model.DeviceName);
                objCmd.Parameters.AddWithValue("@DeviceToken", model.DeviceToken);
                objCmd.Parameters.AddWithValue("@OSVersion", model.OSVersion);
                objCmd.Parameters.AddWithValue("@TimeZoneId", model.TimeZoneId);
                objCmd.Parameters.AddWithValue("@ParkingLocationId", model.ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);



                DataSet ds = objSQL.FetchDB(objCmd);
                var appVersion = (from DataRow dr in ds.Tables[0].Rows
                                  select new AppVersionResponse
                                  {
                                      AppVersionCode = Convert.ToString(dr["AppVersionCode"]),
                                      IsMandatoryUpdate = Convert.ToBoolean(dr["IsMandatoryUpdate"])
                                  }).FirstOrDefault();

                appVersion.BadgeCount = ds.Tables[1].Rows.Count > 0 ? Convert.ToInt64(ds.Tables[1].Rows[0]["BadgeCount"]) : 0;
                return appVersion;
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

        public List<VehicleManufacturerMst> GetManufacturerMaster()
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetManufacturerMaster");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);
                var vehicle = (from DataRow dr in ds.Tables[0].Rows
                               select new VehicleManufacturerMst
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   Name = Convert.ToString(dr["Name"])
                               }).ToList();

                return vehicle;
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

        public List<VehicleColorMst> GetColorMaster()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetColorMaster");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);
                var vehicle = (from DataRow dr in ds.Tables[0].Rows
                               select new VehicleColorMst
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   Name = Convert.ToString(dr["Name"])
                               }).ToList();

                return vehicle;
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

        public List<VehicleTypeMst> GetVehicleTypes()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetVehicleTypes");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);
                var vehicle = (from DataRow dr in ds.Tables[0].Rows
                               select new VehicleTypeMst
                               {
                                   Id = Convert.ToInt32(dr["Id"]),
                                   Name = Convert.ToString(dr["Name"]),
                                   IsOverWeight = Convert.ToBoolean(dr["IsOverWeight"])
                               }).ToList();

                return vehicle;
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

        public void UpdateProfilePic(ProfilePicRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateProfilePic");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", model.UserId);
                objCmd.Parameters.AddWithValue("@ProfilePic", model.ProfilePic);

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

        public string GetOtp(string Mobile)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOtp");
            try
            {
                objCmd.Parameters.AddWithValue("@Mobile", parkingHelper.GetMobileWithoutSpecialCharacter(Mobile));
                DataTable dtOtp = objSQL.FetchDT(objCmd);

                if (dtOtp.Rows.Count > 0)
                    return Convert.ToString(dtOtp.Rows[0][0]);
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

        public VehicleMasterResponse GetVehicleMasterData()
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetVehicleMasterData");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);

                var vehicleManufacturerMsts = (from DataRow dr in ds.Tables[0].Rows
                                               select new VehicleManufacturerMst
                                               {
                                                   Id = Convert.ToInt32(dr["Id"]),
                                                   Name = Convert.ToString(dr["Name"])
                                               }).ToList();


                var vehicleColorMsts = (from DataRow dr in ds.Tables[1].Rows
                                        select new VehicleColorMst
                                        {
                                            Id = Convert.ToInt32(dr["Id"]),
                                            Name = Convert.ToString(dr["Name"])
                                        }).ToList();

                var vehicleTypeMsts = (from DataRow dr in ds.Tables[2].Rows
                                       select new VehicleTypeMst
                                       {
                                           Id = Convert.ToInt32(dr["Id"]),
                                           Name = Convert.ToString(dr["Name"]),
                                           IsOverWeight = Convert.ToBoolean(dr["IsOverWeight"])
                                       }).ToList();

                var CountriesMsts = (from DataRow dr in ds.Tables[3].Rows
                                     select new Countries
                                     {
                                         Id = Convert.ToInt64(dr["Id"]),
                                         Name = Convert.ToString(dr["Name"]),
                                         CountryCode = Convert.ToString(dr["CountryCode"])
                                     }).ToList();

                var statesMst = (from DataRow dr in ds.Tables[4].Rows
                                 select new StatesMst
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     Name = Convert.ToString(dr["Name"]),
                                     StateCode = Convert.ToString(dr["StateCode"]),
                                     CountryId = Convert.ToInt64(dr["CountryId"]),
                                     CountryCode = Convert.ToString(dr["CountryCode"])
                                 }).ToList();


                return new VehicleMasterResponse
                {
                    ListManufacturer = vehicleManufacturerMsts,
                    ListColor = vehicleColorMsts,
                    ListVehicleType = vehicleTypeMsts,
                    ListCountries = CountriesMsts,
                    ListStates = statesMst
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

        public void InsertIntoTempTable(CameraFrameResponse model)
        {
            var Text = JsonSerializer.Serialize(model);
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InsertIntoTempTable");
            try
            {
                objCmd.Parameters.AddWithValue("@Text", Text);
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

        public string ImageUpload(IFormFile file)
        {
            var folderName = Path.Combine("wwwroot", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (file.Length > 0)
            {
                if (!(Directory.Exists(pathToSave)))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension))
                    fileName = fileName + ".png";
                string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                // var fullPath = strpath + (date + file.FileName);
                var fullPath = Path.Combine(pathToSave, date + fileName);
                var dbPath = Path.Combine(folderName, date + fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return pathToSave + "\\" + date + fileName;
            }
            return string.Empty;

        }

        public DetectedVehiclesInfo CheckBookingForDetectedVehicles(List<VehicleNumber> ListVehicles, string CameraId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetTimeZoneByCameraId");
            try
            {
                objCmd.Parameters.AddWithValue("@CameraId", CameraId);
                DataTable dtTz = objSQL.FetchDT(objCmd);

                var TimeZone = (from DataRow dr in dtTz.Rows
                                select Convert.ToString(dr["Name"])
                                     ).FirstOrDefault();

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);

                DateTime ReportedTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

                objCmd = new SqlCommand("sp_CheckBookingForDetectedVehicles");
                objCmd.Parameters.AddWithValue("@CameraId", CameraId);
                objCmd.Parameters.AddWithValue("@ReportedTime", ReportedTime);
                objCmd.Parameters.AddWithValue("@NumberPlatesRef", MapDataTable.ToDataTable(ListVehicles));

                DataSet ds = objSQL.FetchDB(objCmd);

                var BookingStatus = (from DataRow dr in ds.Tables[0].Rows
                                     select new BookingStatus
                                     {
                                         Status = Convert.ToBoolean(dr["Status"]),
                                         NumberPlate = Convert.ToString(dr["NumberPlate"])
                                     }).ToList();

                var LocationInfo = (from DataRow dr in ds.Tables[1].Rows
                                    select new LocationCameraInfo
                                    {
                                        ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                        LocationName = Convert.ToString(dr["LocationName"]),
                                        IsForEntry = Convert.ToBoolean(dr["IsForEntry"])
                                    }).FirstOrDefault();
                return new DetectedVehiclesInfo { BookingStatus = BookingStatus, LocCameraInfo = LocationInfo, CurrentDate = ReportedTime };
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

        public RecognizedVehicleList GetRecognizedVehicleList(long ParkingLocationId, DateTime CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetRecognizedVehicleList");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataTable dtVehicles = objSQL.FetchDT(objCmd);

                List<RecognizedVehicle> recognizedVehicles = new List<RecognizedVehicle>();


                var ListVehicles = (from DataRow dr in dtVehicles.Rows
                                    select new RecognizedVehicle
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        CustomerBookingId = dr["CustomerBookingId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerBookingId"]),
                                        CustomerInfoId = dr["CustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerId"]),
                                        CustomerVehicleId = dr["CustomerVehicleId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["CustomerVehicleId"]),
                                        HasEntered = Convert.ToBoolean(dr["HasEntered"]),
                                        IsBookingFound = Convert.ToBoolean(dr["IsBookingFound"]),
                                        NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                        ShowCheckInButton = IsButtonShow(Convert.ToBoolean(dr["IsBookingFound"]), Convert.ToBoolean(dr["HasEntered"]), EButtonVariant.Entry.ToString()),
                                        ShowCheckOutButton = IsButtonShow(Convert.ToBoolean(dr["IsBookingFound"]), Convert.ToBoolean(dr["HasEntered"]), EButtonVariant.Exit.ToString()),
                                        ShowGuestBookingButton = IsButtonShow(Convert.ToBoolean(dr["IsBookingFound"]), Convert.ToBoolean(dr["HasEntered"]), EButtonVariant.Guest.ToString()),

                                    }).ToList();

                return new RecognizedVehicleList { ListVehicles = ListVehicles };
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

        public bool IsButtonShow(bool IsBookingFound, bool HasEntered, string ButtonVariant)
        {

            if (ButtonVariant.Equals(EButtonVariant.Guest.ToString()) && !IsBookingFound)
                return true;
            if (ButtonVariant.Equals(EButtonVariant.Entry.ToString()) && !HasEntered && IsBookingFound)
                return true;
            if (ButtonVariant.Equals(EButtonVariant.Exit.ToString()) && HasEntered)
                return true;

            return false;

        }

        public ConversationListResponse GetConversationList(long NotificationId, bool IsFromCustomer)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetConversationList");
            try
            {
                objCmd.Parameters.AddWithValue("@NotificationId", NotificationId);
                objCmd.Parameters.AddWithValue("@IsFromCustomer", IsFromCustomer);

                DataTable dtConversation = objSQL.FetchDT(objCmd);
                var ListConversation = (from DataRow dr in dtConversation.Rows
                                        select new Conversation
                                        {
                                            Id = Convert.ToInt64(dr["Id"]),
                                            Message = Convert.ToString(dr["Message"]),
                                            IsFromCustomer = Convert.ToBoolean(dr["IsFromCustomer"]),
                                            ConversationDate = Convert.ToDateTime(dr["ConversationDate"])
                                        }).ToList();

                return new ConversationListResponse { ConversationList = ListConversation };
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

        public long GetUnreadCount(long UserId, long? ParkingLocationId, DateTime CurrentDate, bool IsFromValetApp)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUnreadCount");
            try
            {
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);
                objCmd.Parameters.AddWithValue("@IsFromValetApp", IsFromValetApp);
                DataTable dtCount = objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtCount.Rows[0]["UnreadCount"]);
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

        public void UpdatePaypalCustomerId(UpdatePaypalCustomerIdModel model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdatePaypalCustomerId");
            try
            {
                objCmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                objCmd.Parameters.AddWithValue("@PaypalCustomerId", model.PaypalCustomerId);
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
    }
}