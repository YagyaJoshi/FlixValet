using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.ParkingLocationModels;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
	public interface ICustomer
	{

		long AddCustomerInfo(CustomerInfo model);

		EditCustomerInfoRequest GetCustomerInfoById(long CustomerId);

		long AddCustomerVehicles(CustomerVehicles model);

		List<CustomerVehicleResponse> GetCustomerVehicles(long CustomerInfoId);

		CustomerVehicleResponse GetVehicleInfoById(long Id);

		long UpdateNotificationMode(UpdateNotificationRequest model);

		long AddNotification(Notification model);
		List<NotificationResponse> GetNotifications(long CustomerInfoId, long ParkingLocationId);
		(long, string) AddGuestUser(GuestUserRequest model);

		(long, string) ResendOTP(CustomerIdModel model);

		long VerifyOTP(VerifyOTPRequest model);
		long VerifyGuestVehicle(long CustomerId, string NumberPlate);
		UpcomingBookingResponse GetUpcomingBookings(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate);

		BookingDetailResponse GetBookingDetails(long Id, DateTime CurrentDate);

		EnterRequestDetails CustomerEnterToLocation(CustomerEnterRequest model);

		long CustomerExitFromLocation(CustomerExitRequest model);

		CustomerIdModel GetCustomerIdByVehicleNumber(string VehicleNumber);

		List<VehicleBookingResponse> GetBookingIdByVehicleNumber(string ParkingLocationId, DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit);

		Tuple<List<VehicleBookingResponse>, WhiteListCustomers, BusinessOfficeEmployeeDetails> GetBookingIdByVehicleNumberV1(string ParkingLocationId, DateTime CurrentDate, string VehicleNumber, string CustomerName, bool IsExit);
		//  ExtraChargesResponse CalculateExtraCharges(VehicleBookingResponse bookingResponse);

		ParkedVehiclesResponse GetParkedVehicles(string sortColumn, string sortOrder, int pageNo, int? pageSize, string LocationsList, string SearchValue, string SearchDate);

		long AddDamageVehicle(VehicleDamage model);
		long AddCustomerAddress(CustomerAddress model);
		CustomerAddress GetCustomerAddressById(long Id);
		string CancelBooking(CancelBookingRequest model);

		BookingDetailsByIdResponse GetBookingDetailsById(long Id);

		long UpdateGuestUserDetails(GuestUserDetailRequest model);

		CustomerBookingListResponse GetCustomerBookingList(string sortColumn, string sortOrder, int pageNo, int? pageSize, long CustomerId, string BookingType, DateTime SearchDate, string SearchMode);

		bool MakeAdditionalPayment(AdditionalPaymentRequest model, string paymenturl);

		void EditCustomerInfo(EditCustomerInfoRequest model);

		long DeleteCustomerVehicle(CustomerVehicleIdmodel model);
		BookingDetailsByBIdResponse GetBookingDetailsByBookingId(long BookingId);

		CustomerNotificationModel GetNotificationByCustomerId(long CustomerId, string sortColumn, string sortOrder, int pageNo, int? pageSize);

		PushNotificationModel RequestVehicle(RequestVehicleModel model);

		CustomerPaymentDetails GetCustomerPaymentDetails(long CustomerId, long ParkingLocationId, long? CustomerVehicleId = null);

		BookingPaymentDetails GetBookingPaymentDetails(long CustomerBookingId, DateTime CurrentDate);

		long DeleteVehicleDamageReport(VehicleDamageIdModel model);

		long ChangeVehicleForBooking(ChangeVehicleRequest model);

		// ElectronicPaymentDetails GetElectronicPaymentDetails(long CustomerBookingId, DateTime CurrentDate);
		ElectronicPaymentDetails GetElectronicPaymentDetails(long CustomerBookingId);
		void MakeElectronicPayment(ElectronicPaymentRequest model, object PaymentInfo = null);
		List<VehicleListResponse> GetParkedVehicleListByCustomerId(long CustomerId);
		CustomerBookingDetailsResponse GetBookingDetailsByCustomer(long BookingId);

		CheckCustomerDueAmount CheckCustomerDueAmount(long BookingId);

		GuestIDResponse AddGuestDetails(GuestDetailsRequest model, string Otp);

		(long, string) SetGuestOtp(SetGuestOtpRequest model);

		void UpdateNotificationStatus(UpdateNotificationStatusModel model);

		long AddNotificationbyStaff(StaffNotificationModel model, NotificationDetails NotificationDetails);

		NotificationDetails GetNotificationDetails(StaffNotificationModel model);

		long StaffCustConversation(StaffNotificationModel model, NotificationDetails NotificationDetails);

		RecognizedVehicleListResponse GetRecognizedVehicleList(long ParkingLocationId, string sortColumn, string sortOrder, int pageNo, int? pageSize, string SearchValue, DateTime CurrentDate, string CameraId);


		GuestList FetchGuestfromVehicle(string NumberPlate);

		GuestListv1 FetchGuestfromVehiclev1(string NumberPlate);

		List<VehicleBookingResponse> ScanReceipt(long CustomerBookingId, bool IsExit);

		(long, long) BrowserLaunch(BrowserLaunchRequest model);

		void InsertIntoLogFile(string Message);


		Task PostBookingActions(PostBookingModel model, BookingRequest bookingRequest, decimal Amount);

		CustomerDetailsResponse GetCustomerDetails(long CustomerId);

		PreBookingDetailsResponse GetPreBookingDetails(long ParkingLocationId, long CustomerId);

		void SavePostBookingDetails(PostBookingSaveModel model);

		bool SendBookingConfirmationtoCustomer(PostBookingModel postBookingModel, BookingRequest bookingRequest, string CustomerMessage, string Message);

		bool SendBookingConfirmationtoOwnerStaff(PostBookingModel postBookingModel, BookingRequest bookingRequest, string ValetMessage, ref bool IsNotificationRequired);
		FetchCustomerDetailsResponse FetchCustomerDetails(FetchCustomerDetailsRequest model, string origin);
		ExtendBookingDetailsResponse GetExtendBookingDetails(long CustomerBookingId, long CustomerId, DateTime CurrentDate);
		SearchCustomersFromFilterResponse SearchCustomersFromFilter(string Email, string Mobile);

		void UpdateQRCodePath(UpdateQRCodeModel model);
		Task UpdateQRCode(long BookingId, String LogoUrl);
		long AddCustomer(AddCustomerRequest model, string origin);
		long AddWhiteListCustomer(WhiteListCustomers model);
		long AddPOBusinessOfficeEmployee(POBusinessOfficeEmployees model);
		long AddPOBusinessOfficeEmployee_v1(POBusinessOfficeEmployeeInput model);

        WhiteListCustomerDetailResponse GetWhiteListCustomerById(long WhiteListCustomerId);
		OfficeEmployeeDetailsResponse GetPOBusinessOfficeEmployeeById(long BusinessOfficeEmployeeId);
		WhiteListCustomerListResponse GetWhiteListCustomerList(int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue, long ParkingBusinessOwnerId);
        POBusinessOfficeEmployeeList GetPOBusinessOfficeEmployeeList(long ParkingBusinessOwnerId, long? BusinessOfficeId, int PageNo, int? PageSize, string SortColumn, string SortOrder, string SearchValue);
		bool CheckWhiteListVehicleExists(long? WhiteListCustomerId, string NumberPlate);

		bool CheckBuisnessOfficeEmployeeExists(long? BusinessOfficeEmployeeId, long CustomerVehicleId, long BusinessOfficeId);
		void UpdateOfficeEmployeePayment(OfficeEmployeeListModel model);
		void UpdateGuestMobile(UpdateGuestMobileModel model);

		void MakeAdditionPaymentFromQRScan(AdditionalPaymentFromQRRequest model, object PaymentInfo = null);

		FetchCustomerFromEmailAndMobileResponse FetchCustomerFromEmailAndMobile(FetchCustomerFromEmailAndMobileRequest model);


        BookingResponse GetBookingsByVehicleNumber(string sortColumn, string sortOrder, int? pageNo, int? pageSize, long? LocationId, string? SearchValue, string? StartDate, string? EndDate);


        IEnumerable<ParkingLocationDto> GetParkingLocationsByOwner();
    }

}