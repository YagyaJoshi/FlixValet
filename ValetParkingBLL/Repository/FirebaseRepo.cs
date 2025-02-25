using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;

namespace ValetParkingBLL.Repository
{
    public class FirebaseRepo : IFirebase
    {
        private readonly IConfiguration _config;
        private readonly IStaff _staffRepo;

        private readonly FirebaseSettings _firebase;

        public FirebaseRepo(IConfiguration config, IStaff staffRepo)
        {
            _config = config;
            _staffRepo = staffRepo;
            _firebase = _config.GetSection("firebase").Get<FirebaseSettings>();
        }
        public string SendFCMNotification(object data, bool IsFromCustomer = false)
        {
            try
            {
                string SERVER_API_KEY, SENDER_ID;
                var json = JsonSerializer.Serialize(data);
                Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);

                SERVER_API_KEY = IsFromCustomer ? _firebase.CustomerAppAPIKey : _firebase.ServerAPIKey;

                SENDER_ID = IsFromCustomer ? _firebase.CustomerAppSenderId : _firebase.SenderId;

                WebRequest tRequest;
                tRequest = WebRequest.Create(_firebase.BaseUrl);
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

                tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

                tRequest.ContentLength = byteArray.Length;
                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse tResponse = tRequest.GetResponse();

                dataStream = tResponse.GetResponseStream();

                StreamReader tReader = new StreamReader(dataStream);

                String sResponseFromServer = tReader.ReadToEnd();

                tReader.Close();
                dataStream.Close();
                tResponse.Close();



                return sResponseFromServer;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string SendNotificationtoStaff(long ParkingLocationId, string Title, string NotificationMsg, DateTime CurrentDate)
        {
            var (ReceiverTokenIds, BadgeCount) = _staffRepo.GetDeviceTokensofLocationStaff(ParkingLocationId, CurrentDate);
            // if (ReceiverTokenIds == null || ReceiverTokenIds.Count == 0)
            //     throw new AppException("Notification is not enabled at this location");

            dynamic data = new
            {
                registration_ids = ReceiverTokenIds,
                notification = new
                {
                    title = Title,
                    body = NotificationMsg,
                    sound = "default"
                },
                data = new
                {
                    badge = Convert.ToString(BadgeCount + 1), //With new request, notification addition takes place later but badgecount should be incremented
                    vehicle_number = Title.Contains("-") ? Title.Split('-')[1].Trim() : ""
                },
                priority = "high",
                // contentAvailable= true
            };
            return SendFCMNotification(data);

        }

        public string SendVehicleRequestNotifications(PushNotificationModel model, string Title, string NotificationId)
        {
            // var (ReceiverTokenIds, BadgeCount) = _staffRepo.GetDeviceTokensofLocationStaff(model.ParkingLocationId);

            //   if (ReceiverTokenIds == null || ReceiverTokenIds.Count == 0)
            //  return null;

            try
            {
                dynamic data = new
                {
                    registration_ids = model.DeviceTokens,
                    notification = new
                    {
                        title = Title,
                        body = model.NotificationMessage,
                        sound = "default"
                    },
                    data = new
                    {
                        badge = Convert.ToString(model.BadgeCount + 1),//With new request, notification addition takes place later but badgecount should be incremented
                        NotificationId = NotificationId
                    },
                    priority = "high",
                    //   contentAvailable = true
                };

                #region old code
                // registration_ids = ReceiverTokenIds,
                // notification = new
                // {
                //     title = Title,
                //     body = NotificationMsg,
                //     badge = Convert.ToString(BadgeCount+1) //With new request, notification addition takes place later but badgecount should be incremented

                // }
                // };
                #endregion
                return SendFCMNotification(data);
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public string SendNotificationtoCustomer(string[] DeviceTokens, long BadgeCount, string Title, string NotificationMsg)
        {
            string fcmresponse = null;
            FirebaseCustomerRequest data = null;

            data = new FirebaseCustomerRequest
            {
                to = DeviceTokens[0],
                notification = new FNotification
                {
                    title = Title,
                    body = NotificationMsg,
                    sound = "default"
                },
                data = new FData
                {
                    badge = Convert.ToString(BadgeCount + 1), //With new request, notification addition takes place later but badgecount should be incremented
                    vehicle_number = Title.Contains("-") ? Title.Split('-')[1].Trim() : ""
                },
                priority = "high",
                // contentAvailable = true
            };

            if (!string.IsNullOrEmpty(DeviceTokens[0]))
                fcmresponse = SendFCMNotification(data, true);


            if (!string.IsNullOrEmpty(DeviceTokens[1]))
            {
                data.to = DeviceTokens[1];
                fcmresponse = SendFCMNotification(data, true);
            }

            return fcmresponse;
        }
    }
}