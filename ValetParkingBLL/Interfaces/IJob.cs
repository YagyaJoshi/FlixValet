using System;
using System.Collections.Generic;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.JobModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IJob
    {
        PendingStatusRequest GetRefundPendingStatusList();
        void UpdatePendingStatus(PendingStatusRequest model);
        void DeleteArchiveAnpr(ArchiveAnprModel model);

        RemindingListModel GetRemindingList(DateTime CurrentDate);

        void SaveReminderNotification(NotificationListModel notificationList);

        MonthlyBookingDetails GetMonthlyBookingsExpiringToday(DateTime CurrentDate);
        List<StaffDetails> GetStaffByLocationId(long locationId);

        List<ParkingBusinessOwnerDetails> GetAllParkingOwners();

        List<POBusinessOffice> GetAllBusinessOffice(long parkingBusinessOwnerId);

        List<ChargeBackCustomerDetails> ChargeBackCustomerBookingDetails(long businessOfficeId, int month, int year);

        void SaveChargeBackReport(ChargeBackReportInput input);

    }
}