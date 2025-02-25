
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.NumberPlateRecogModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IMaster
    {
        List<ParkingLocationName> GetLocationsByUser(long ParkingBusinessOwnerId, long UserId);
        long EditUserProfile(ProfileEdit model);
        ProfileEdit GetUserProfileDetails(long UserId);
        AppVersionResponse GetAppVersion(AppVersionModel model);
        List<VehicleManufacturerMst> GetManufacturerMaster();
        List<VehicleColorMst> GetColorMaster();
        List<VehicleTypeMst> GetVehicleTypes();

        VehicleMasterResponse GetVehicleMasterData();

        void UpdateProfilePic(ProfilePicRequest model);

        string GetOtp(string Mobile);

        void InsertIntoTempTable(CameraFrameResponse model);

        string ImageUpload(IFormFile file);

        DetectedVehiclesInfo CheckBookingForDetectedVehicles(List<VehicleNumber> ListVehicles, string CameraId);


        RecognizedVehicleList GetRecognizedVehicleList(long ParkingLocationId, DateTime CurrentDate);

        ConversationListResponse GetConversationList(long NotificationId, bool IsFromCustomer);

        long GetUnreadCount(long UserId, long? ParkingLocationId, DateTime CurrentDate, bool IsFromValetApp);

        void UpdatePaypalCustomerId(UpdatePaypalCustomerIdModel model);

    }
}