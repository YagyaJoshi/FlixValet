using System;
using System.Collections.Generic;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IStaff
    {
        long AddStaff(CreateUserRequest model, string origin);

        StaffMembersResponse GetAllStaffMembers(long ParkingBusinessOwnerId, long UserId, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue);

        long DeleteStaff(CommonId model);

        ParkingBusinessOwnerResponse GetAllParkingBusinessOwners(string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue);


        long AddParkingBusinessOwner(ParkingOwnerRequest model, string origin);

        ParkingOwnerRequest GetParkingOwnerById(long ParkingBusinessOwnerId);

        CreateUserRequest GetStaffById(long UserId);


        StaffCheckinOut CheckIn(StaffCheckinOut model);
        long CheckOut(StaffCheckOutRequest model);
        StaffCheckinOut GetCheckInDetails(long Id);
        InspectCheckInResponse InspectUserCheckIn(long UserId);


        (List<string>, long) GetDeviceTokensofLocationStaff(long ParkingLocationId, DateTime CurrentDate);

        ManagerNotificationModel GetNotificationListForLocation(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, DateTime SearchDate);

        CheckInCheckOut GetCheckInOutListByUser(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string StartDate, string EndDate);

        DamageVehicleResponse GetDamageVehicleByParkingOwner(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue);

        List<GetStaffByOwnerResponse> GetStaffByParkingOwner(long ParkingBusinessOwnerId);

        DamageVehicleDetailsResponse GetDamageVehicleDetails(long DamageVehicleId);

        UnDepositedPaymentResponse GetUndepositedPaymentList(long UserId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string CurrentDate);
        decimal UpdateDepositedPayment(UpdateDepositedPayment model);
        long AddOwnerPaymentSettings(OwnerPaymentSettings model);
        OwnerPaymentSettings GetOwnerPaymentSettings(long ParkingBusinessOwnerId);
        long ChangeStaffActiveStatus(StaffActiveInActiveRequest model);
        (long, bool) DeleteParkingBusinessOwner(ParkingOwnerIdModel model);
    }
}