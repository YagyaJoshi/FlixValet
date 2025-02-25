using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingAPI.Resources
{
	public class Resource
	{
		public string AddSuccessful { get; set; }
		public string ChangePassword { get; set; }

		public string DeleteSuccessful { get; set; }

		public string EmailVerified { get; set; }

		public string ForgotPassword { get; set; }
		public string RegisterSuccessful { get; set; }
		public string RequestSuccessful { get; set; }
		public string ResetPassword { get; set; }
		public string RoleExist { get; set; }
		public string SetPassword { get; set; }
		public string UpdateSuccessful { get; set; }
		public string UserExist { get; set; }
		public string VerifyOtp { get; set; }
		public string VerifyOtpFailed { get; set; }
		public string BookingMessage { get; set; }
		public string AppVersionMessage { get; set; }
		public string VehicleAdded { get; set; }
		public string VehicleUpdated { get; set; }
		public string CustomerAdded { get; set; }
		public string CustomerUpdated { get; set; }
		public string NotificationAdded { get; set; }
		public string CustomerAddressAdd { get; set; }
		public string CustomerAddressUpdate { get; set; }
		public string CancelBooking { get; set; }
		public string NotificationModeUpdate { get; set; }
		public string AddGuestUser { get; set; }
		public string CheckIn { get; set; }
		public string CheckOut { get; set; }
		public string EnterToLocation { get; set; }

		public string ExitFromLocation { get; set; }
		public string AddDamageVehicle { get; set; }
		public string UpdateDamageVehicle { get; set; }
		public string BookParkingLocation { get; set; }
		public string AddParkingBusinessOwner { get; set; }
		public string UpdateParkingBusinessOwner { get; set; }
		public string AddParkingLocation { get; set; }
		public string UpdateParkingLocation { get; set; }
		public string AddStaff { get; set; }
		public string UpdateStaff { get; set; }
		public string login { get; set; }
		public string ResendOTP { get; set; }
		public string AccountInactive { get; set; }
		public string AccountNotVerified { get; set; }
		public string AtleastOneFieldRequired { get; set; }
		public string AppVersionNameRequired { get; set; }
		public string BookingIdRequired { get; set; }
		public string BookingNotFound { get; set; }
		public string CustomerIdRequired { get; set; }
		public string CustomerAndLocationIdRequired { get; set; }
		public string EmailOrPasswordIncorrect { get; set; }
		public string EmailRequired { get; set; }
		public string GuestUser { get; set; }
		public string Image { get; set; }
		public string InvalidToken { get; set; }
		public string LocationDetailsNotFound { get; set; }
		public string LocationNotFound { get; set; }
		public string LocationRates { get; set; }
		public string LocationTiming { get; set; }
		public string ModeOfSearch { get; set; }
		public string NoAdditionalCharges { get; set; }
		public string NoBooking { get; set; }
		public string NotificationsNotFound { get; set; }
		public string NoVehicleParked { get; set; }
		public string NoUpcomingBookings { get; set; }
		public string ParkingBusinessOwnerIdRequired { get; set; }
		public string OwnerAndUserRequired { get; set; }
		public string RecordNotFound { get; set; }
		public string ParkingLocationIdRequired { get; set; }
		public string UpToDate { get; set; }
		public string UnableToBook { get; set; }
		public string UserNotFound { get; set; }
		public string UserIdRequired { get; set; }
		public string VehicleNumberRequired { get; set; }
		public string VehicleIdRequired { get; set; }
		public string BaseUrl { get; set; }
		public string IsExitBookingFailed { get; set; }
		public string IsEnterBookingFailed { get; set; }
		public string ExtraChargesNotApplied { get; set; }
		public string ExtraChargesApplied { get; set; }
		public string ElectronicLinkSuccess { get; set; }

		public string CustomerNotFound { get; set; }

		public string ProfileUploadSuccessful { get; set; }

		public string VehicleDeleteSuccessful { get; set; }

		public string DateRangeError { get; set; }

		public string MonthlyBooking30DayDiffError { get; set; }
		public string VehicleAlreadyRegistered { get; set; }

		public string RequestVehicleMessage { get; set; }
		public string NotificationSent { get; set; }
		public string NotificationFailed { get; set; }
		public string DepositPaymentStatus { get; set; }
		public string NoCashStatus { get; set; }
		public string NotificationDisabled { get; set; }
		public string PaymentSettingsAdded { get; set; }
		public string PaymentSettingsUpdated { get; set; }

		public string PaymentUnsuccessful { get; set; }

		public string ImageUploaded { get; set; }
		public string ExitByTime { get; set; }
		public string EnterFromTime { get; set; }
		public string DamageIdRequired { get; set; }
		public string OTP { get; set; }
		public string LastDepositedMessage { get; set; }
		public string InValidLicenseExpiry { get; set; }
		public string AdminBaseUrl { get; set; }
		public string AlreadyParkedandExited { get; set; }
		public string NoVehicleIsParked { get; set; }
		public string UpdateProfile { get; set; }
		public string EnterAndExitDateTime { get; set; }
		public string EnterAndExitTime { get; set; }
		public string CancelBookingv1 { get; set; }
		public string EBFromtimeMissing { get; set; }
		public string EBTotimeMissing { get; set; }
		public string EBExittimeMissing { get; set; }
		public string CouldntloadImg { get; set; }
		public string NPRecognitionError { get; set; }

		public string VehicleArrivalMessage { get; set; }
		public string VehicleDepartureMessage { get; set; }

		public string NotificationUpdated { get; set; }

		public string NoRequestInitiated { get; set; }

		public string AcknowledgementSent { get; set; }

		public string RequestAccepted { get; set; }
		public string NotificationCustDisabled { get; set; }
		public string LocationCameraIdRequired { get; set; }
		public string CustomerEnterMessage { get; set; }
		public string VehicleAlreadyExited { get; set; }
		public string OwnerActiveInactiveMessage { get; set; }
		public string CameraIdRequired { get; set; }

		public string TigerQRNotGenerated { get; set; }

		public string BookingConfirmedMsgToCustomer { get; set; }

		public string BookingConfirmedTitleToCustomer { get; set; }
		public string BookingConfirmedTitleToValet { get; set; }
		public string BookingConfirmedMsgToValet { get; set; }
		public string DateRequired { get; set; }
		public string FilterRequired { get; set; }
		public string StartTimeAndEndTimeError { get; set; }

		public string MobileOrEmail { get; set; }
		public string InvalidStartTime { get; set; }
		public string InvalidEndTime { get; set; }
		public string InvalidStartDate { get; set; }
		public string InvalidEndDate { get; set; }
		public string WhiteListCustomerAdded { get; set; }
		public string WhiteListCustomerUpdated { get; set; }
		public string EmployeeAdded { get; set; }
		public string EmployeeUpdated { get; set; }
		public string BusinessOfficeAdded { get; set; }
		public string BusinessOfficeUpdated { get; set; }
		public string BusinessOfficeIdRequired { get; set; }
		public string BusinessOfficeEmployeeExists { get; set; }
		public string UpdateOfficeEmployeePayment { get; set; }
		public string GateSettingsAdded { get; set; }
		public string GateSettingsUpdated { get; set; }

		public string GateOpenSuccess { get; set; }
		
		public string MonthlyBookingMessage { get; set; }
		
	}
}
