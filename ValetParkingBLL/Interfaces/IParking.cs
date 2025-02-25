using System;
using System.Collections.Generic;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.QRModels;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
	public interface IParking
	{
		long AddParkingLocation(ParkingLocationRequest model);

		ParkingLocationRequest GetParkingLocationDetails(long Id);

		ParkingLocationsResponse GetAllParkingLocations(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue);

		List<ParkingLocationName> GetParkingLocationsByOwner(long ParkingBusinessOwnerId);

		long DeleteParkingLocation(ParkingLocationIdModel model);

		List<RequestParkingLocations> SearchParkingLocations(CurrentLocationRequest model);

		BookingIdResponse BookParkingLocation(BookingRequest model, object PaymentInfo = null);

		GuestPrebookingResponse GetAvailableParkingDetails(GuestPreBookingRequest model);
		long UpdateNotesforParkingSlot(UpdateNotesRequest model);

		ParkingLocDetailsResponse GetEstimatedBookingDetailsByLoc(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, string BookingType, bool IsFullTimeBooking, long? CustomerVehicleId, string TimeZone, bool IsFromQRScan);

		BookingsByOwnerResponse GetBookingByParkingOwner(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BookingType);

		List<ParkingLocationName> GetLocationNamesFromList(string Locations);

		CheckInReportResponse GetCheckInReport(long ParkingLocationId, string CurrentDate, string SearchValue, bool HasCheckedIn);
		
		List<DamageVehicleReport> GetDamageVehicleReport(long ParkingLocationId, string CurrentDate, string SearchValue);
		
		DepositReportResponse GetDepositReport(string ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue);

		long AddLocationCameraSettings(LocationCameraSettings model);

		LocationCameraSettings GetLocationCameraSettings(long LocationCameraId);
		
		List<CameraIdListResponse> GetCameraListByLocation(long ParkingLocationId);
		
		CameraSettingListResponse GetCameraSettingList(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue);

		long DeleteCameraSettings(LocationCameraIdModel model);

		string GetLocationQRCode(long ParkingLocationId, string LogoUrl);

		string GenerateDynamicQR(DynamicQRCodeModel model);

		PostBookingModel BookParkingLocation_v1(BookingRequest model, object PaymentInfo = null);

        PostBookingModel BookParkingLocationFromQR(BookingFromQrRequest model, string subscriptionId);
        string GetLocationStaticQRCode(long ParkingLocationId);

		QRListResponse GetLocationQRList(long ParkingBusinessOwnerId, string sortColumn, string sortOrder, int pageNo, int? pageSize, bool isMonthly = false);

		List<LocationQRData> GetOwnerLocationsWithoutQR(long ParkingBusinessOwnerId, bool IsMonthly = false);
		BookingExtensionPendingResponse GetBookingExtensionPendingList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string CurrentDate);

		BookingsByOwnerResponse BookingRevenueReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string BookingType, string StartDate, string EndDate);

		BookingsByOwnerResponse GetBookingRevenueCSVReport(long ParkingBusinessOwnerId, long? ParkingLocationId, string BookingType, string StartDate, string EndDate);

        AccountReceivableReportResponse AccountReceivableReport(long? ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, string BusinessOffice, string StartDate, string EndDate, long BookingCategoryId);

		long AddParkingOwnerBusinessOffice(ParkingOwnerBusinessOffices model, string origin = "");


        POBusinessOfficeListResponse GetPOBusinessOfficeById(long BusinessOfficeId);
		List<POBusinessOfficeListResponse> GetPOBusinessOfficeList(long ParkingLocationId);

        POBusinessOfficeList GetAllPOBusinessOfficeList(long? ParkingBusinessOwnerId, long? ParkingLocationId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue);

        long ChangeBusinessOfficeActiveStatus(BusinessOfficeIdModel model);
		
		List<RelayTypes> GetRelayTypes();

		ParkingLocationGateSettingResponse GetAllParkingLocationGateSettings(long ParkingBusinessOwnerId, int pageNo, int? pageSize, string sortColumn, string sortOrder, string searchValue);

		ParkingLocationGateSettings GetParkingLocationGateSettingById(long Id);

		long AddPakingLocationGateSettings(ParkingLocationGateSettings model);

		long DeleteParkingLocationGateSetting(CommonId model);

		ParkingLocationGateSettings GetLocationGateSettingByLocationId(long parkingLocationId);

		void OpenCloseGate(long Id);
		
		List<ParkingLocationName> GetParkingGateSettingLocationList(long ParkingBusinessOwnerId);

		List<ParkingLocationGateNumbersResponse> GetParkingLocationGates(long ParkingLocationId);

        ParkingLocDetailsResponse GetMonthlyQRBookingInfo(long ParkingLocationId, long CustomerId, DateTime StartDate, DateTime EndDate, string StartTime, string EndTime, bool IsFullTimeBooking, long? CustomerVehicleId);

		POBusinessOfficeListResponse GetBusinessOfficeByUserId(long UserId);

        ChargeBackBookingList ChargeBackCustomerBookingReport(long businessOfficeId, int pageNo, int? pageSize, string sortColumn, string sortOrder);

       // QrcodeScanDataResponse GetQrCodeActivitylogs(string qrId, string timeZone, string date);
		QrcodeScanDataResponse GetQrCodeActivitylogs(long parkingLocationId, string bookingType, string date);
    }
}