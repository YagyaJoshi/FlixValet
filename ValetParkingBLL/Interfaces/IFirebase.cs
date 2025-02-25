using System;
using System.Collections.Generic;
using ValetParkingDAL.Models.CustomerModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IFirebase
    {
        string SendVehicleRequestNotifications(PushNotificationModel model, string Title, string NotificationId);
        string SendNotificationtoStaff(long ParkingLocationId, string Title, string NotificationMsg, DateTime CurrentDate);

        string SendNotificationtoCustomer(string[] DeviceTokens, long BadgeCount, string Title, string NotificationMsg);



    }
}