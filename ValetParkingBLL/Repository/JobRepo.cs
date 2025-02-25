using System.Linq;
using System.Collections.Generic;
using ValetParkingDAL.Models;
using System;
using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;
using System.Net;
using System.Data.SqlClient;
using Stripe;
using ValetParkingDAL.Models.JobModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.UserModels;
using ValetParkingDAL.Enums;

namespace ValetParkingBLL.Repository
{
    public class JobRepo : IJob
    {

        private readonly IConfiguration _configuration;

        private readonly AppSettings _appsettings;

        private readonly ISquare _squareRepo;
        private readonly IStaff _staffRepo;
        private readonly StatisticsHelper _statisticsHelper;


        public JobRepo(IConfiguration configuration, ISquare squareRepo, IStaff staffRepo, StatisticsHelper statisticsHelper)
        {
            _configuration = configuration;
            _squareRepo = squareRepo;
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _staffRepo = staffRepo;
            _statisticsHelper = statisticsHelper;
        }

        public PendingStatusRequest GetRefundPendingStatusList()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetRefundPendingList");
            try
            {
                DataSet ds = objSQL.FetchDB(objCmd);

                var refundStatusList = (from DataRow dr in ds.Tables[0].Rows
                                        select new RefundPendingStatus
                                        {
                                            RefundId = Convert.ToString(dr["RefundId"]),
                                            RefundStatus = Convert.ToString(dr["RefundStatus"]),
                                            CustomerBookingId = Convert.ToInt64(dr["CustomerBookingId"]),
                                            PaymentProvider = Convert.ToString(dr["PaymentProvider"])
                                        }).ToList();

                return new PendingStatusRequest
                {
                    RefundStatusList = refundStatusList
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

        public void UpdatePendingStatus(PendingStatusRequest model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdatePendingStatus");
            try
            {
                objCmd.Parameters.AddWithValue("@RefundStatusRef", MapDataTable.ToDataTable(model.RefundStatusList));
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


        public void DeleteArchiveAnpr(ArchiveAnprModel model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteArchiveAnpr");
            try
            {
                objCmd.Parameters.AddWithValue("@CurrentDate", model.CurrentDate);
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

        public RemindingListModel GetRemindingList(DateTime CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetRemindingList");
            try
            {
                objCmd.Parameters.AddWithValue("@CurrentDate", Convert.ToDateTime(CurrentDate.ToString("yyyy-MM-ddTHH:mm")));
                DataSet ds = objSQL.FetchDB(objCmd);
                RemindingListModel rmList = null;

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var reminderList = (from DataRow dr in ds.Tables[0].Rows
                                        select new ReminderDataModel
                                        {
                                            DeviceToken = Convert.ToString(dr["DeviceToken"]),
                                            BrowserDeviceToken = Convert.ToString(dr["BrowserDeviceToken"]),
                                            CustomerId = Convert.ToInt64(dr["CustomerId"]),
                                            ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                            CustomerBookingId = Convert.ToInt64(dr["CustomerBookingId"]),
                                            Mobile = Convert.ToString(dr["Mobile"]),
                                            NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                            CustomerBadgeCount = Convert.ToInt64(dr["CustomerBadgeCount"]),
                                            BookingEndTime = Convert.ToDateTime(dr["BookingEndTime"]),
                                            CustomerVehicleId = Convert.ToInt64(dr["CustomerVehicleId"])

                                        }).ToList();


                    var staffList = (from DataRow dr in ds.Tables[1].Rows
                                     select new StaffInfoModel
                                     {
                                         DeviceToken = Convert.ToString(dr["DeviceToken"]),
                                         ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                         UserId = Convert.ToInt64(dr["UserId"])
                                     }).ToList();

                    var locationBadgeList = (from DataRow dr in ds.Tables[2].Rows
                                             select new LocationBadgeCountModel
                                             {

                                                 ParkingLocationId = Convert.ToInt64(dr["ParkingLocationId"]),
                                                 BadgeCount = Convert.ToInt64(dr["BadgeCount"])
                                             }).ToList();



                    rmList = new RemindingListModel
                    {
                        ReminderList = reminderList,
                        StaffTokensList = staffList,
                        LocationBadgeCountList = locationBadgeList
                    };

                }
                return rmList;
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

        public void SaveReminderNotification(NotificationListModel notificationList)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SaveReminderNotification");
            try
            {
                objCmd.Parameters.AddWithValue("@NotificationRef", MapDataTable.ToDataTable(notificationList.NotificationList));
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
        public MonthlyBookingDetails GetMonthlyBookingsExpiringToday(DateTime CurrentDate)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetMonthlyBookingsExpiringToday");
            try
            {
                objCmd.Parameters.AddWithValue("@CurrentDate", CurrentDate);

                DataTable dt = objSQL.FetchDT(objCmd);
                var bookingList = (from DataRow dr in dt.Rows
                                   select new CustomerBookingDetailsList
                                   {
                                       BookingId = Convert.ToInt64(dr["BookingId"]),
                                       CustomerId = Convert.ToInt64(dr["CustomerId"]),
                                       CustomerName = Convert.ToString(dr["CustomerName"]),
                                       StartDate = Convert.ToDateTime(dr["StartDate"]),
                                       EndDate = Convert.ToDateTime(dr["EndDate"]),
                                       Email = Convert.ToString(dr["Email"]),
                                       Mobile = Convert.ToString(dr["Mobile"]),
                                       LocationId = Convert.ToInt64(dr["LocationId"]),
                                       Role = Convert.ToString(dr["Role"]),
                                       RoleId = Convert.ToInt32(dr["RoleId"]),
                                       NumberPlate = Convert.ToString(dr["NumberPlate"])
                                   }).ToList();

                return new MonthlyBookingDetails
                {
                    BookingDetails = bookingList
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

        public List<StaffDetails> GetStaffByLocationId(long locationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStaffByLocationId");
            try
            {
                objCmd.Parameters.AddWithValue("@LocationId", locationId);

                DataTable dt = objSQL.FetchDT(objCmd);
                var staffList = (from DataRow dr in dt.Rows
                                   select new StaffDetails
                                   {
                                       UserId = Convert.ToInt64(dr["UserId"]),
                                       Email = Convert.ToString(dr["Email"]),
                                       Mobile = Convert.ToString(dr["Mobile"]),
                                   }).ToList();
                return staffList;
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

        public List<ParkingBusinessOwnerDetails> GetAllParkingOwners()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllParkingBusinessOwners");
            try
            {
                DataTable dtUsrs = objSQL.FetchDT(objCmd);
                var lstUsers = new List<ParkingBusinessOwnerDetails>();
                if (dtUsrs.Rows.Count > 0)
                {
                    lstUsers = (from DataRow dr in dtUsrs.Rows
                                    select new ParkingBusinessOwnerDetails
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),                                   
                                        Email = Convert.ToString(dr["Email"]),
                                    }).ToList();

                }
                return lstUsers;
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

        public List<POBusinessOffice> GetAllBusinessOffice(long parkingBusinessOwnerId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllBusinessOffice");
            try
            {

                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", parkingBusinessOwnerId);
                DataTable dtUsrs = objSQL.FetchDT(objCmd);
                var lstUsers = new List<POBusinessOffice>();
                if (dtUsrs.Rows.Count > 0)
                {
                    lstUsers = (from DataRow dr in dtUsrs.Rows
                                    select new POBusinessOffice
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        Email = Convert.ToString(dr["Email"]),
                                        LocationName = Convert.ToString(dr["LocationName"])
                                    }).ToList();

                }
                return lstUsers;
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

        public List<ChargeBackCustomerDetails> ChargeBackCustomerBookingDetails(long businessOfficeId, int month, int year)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ChargeBackCustomerBookingDetails");
            try
            {
                objCmd.Parameters.AddWithValue("@BusinessOfficeId", businessOfficeId);
                objCmd.Parameters.AddWithValue("@Month", month);
                objCmd.Parameters.AddWithValue("@Year", year);

                DataTable dtBookings = objSQL.FetchDT(objCmd);
                if (dtBookings.Rows.Count > 0) 
                {
                    var bookings = (from DataRow dr in dtBookings.Rows
                                    select new ChargeBackCustomerDetails
                                    {
                                        BookingType = Convert.ToInt32(dr["BookingTypeId"]) == 1 ? "hourly" : "monthly",
                                        StartDate = Convert.ToDateTime(dr["StartDate"]),
                                        EndDate = Convert.ToDateTime(dr["EndDate"]),
                                        Duration = _statisticsHelper.GetDateDifference(Convert.ToDateTime(dr["StartDate"]), Convert.ToDateTime(dr["EndDate"]), Convert.ToInt32(dr["BookingTypeId"]) == 1 ? false : true),
                                        NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                                        NumberPlate = Convert.ToString(dr["NumberPlate"]),
                                        LocationName = Convert.ToString(dr["LocationName"]),
                                        CustomerName = Convert.ToString(dr["CustomerName"]),
                                        Email = Convert.ToString(dr["Email"])
                                    }).ToList();

                    return bookings;
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

        public void SaveChargeBackReport(ChargeBackReportInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SaveChargeBackCustomerReport");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingBusinessOwnerId", input.ParkingBusinessOwnerId);
                objCmd.Parameters.AddWithValue("@BusinessOfficeId", input.BusinessOfficeId);
                objCmd.Parameters.AddWithValue("@Url", input.Url);
                objCmd.Parameters.AddWithValue("@Type", EReportType.ChargeBack);

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